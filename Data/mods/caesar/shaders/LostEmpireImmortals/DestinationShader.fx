string description = "Basic Vertex Lighting with a Texture";

//------------------------------------
float4x4 viewProjection : viewProjection;
float offset;

texture lineTexture : Diffuse
<
	string ResourceName = "default_color.dds";
>;

float4 lineColor : Color
<
> = {1.0f, 1.0f, 1.0f, 1.0f};




//------------------------------------
struct vertexInput {
    float4 position				: POSITION;
    float2 texCoordDiffuse		: TEXCOORD0;
    float2 offsetSpeed		: TEXCOORD1;
    float4 color			: COLOR0;
};

struct vertexOutput {
    float4 hPosition		: POSITION;
    float2 texCoordDiffuse	: TEXCOORD0;
    float4 color			: COLOR0;
};




//------------------------------------
vertexOutput VS_TransformAndTexture(vertexInput IN) 
{
    vertexOutput OUT;
    OUT.hPosition = mul(IN.position , viewProjection);
    OUT.texCoordDiffuse = IN.texCoordDiffuse;
    OUT.texCoordDiffuse.y+=offset*IN.offsetSpeed.y;
    OUT.color=IN.color;
    return OUT;
}


//------------------------------------
sampler TextureSampler = sampler_state 
{
    texture = <lineTexture>;
    AddressU  = CLAMP;        
    AddressV  = WRAP;
    AddressW  = CLAMP;
    MIPFILTER = LINEAR;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
};


//-----------------------------------
float4 PS_Textured( vertexOutput IN): COLOR
{
  float4 diffuseTexture = tex2D( TextureSampler, IN.texCoordDiffuse );
  return IN.color * diffuseTexture;
}


//-----------------------------------
technique textured
{
    pass p0 
    {		
		VertexShader = compile vs_1_1 VS_TransformAndTexture();
		PixelShader  = compile ps_1_1 PS_Textured();
    }
}