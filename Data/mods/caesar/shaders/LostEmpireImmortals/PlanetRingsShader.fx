string description = "Planet Shader";

//------------------------------------
float4x4 viewProjection : ViewProjection;
float4x4 world : World;

texture diffuseTexture : Diffuse
<
	string ResourceName = "default_color.dds";
>;

//------------------------------------
struct vertexInput {
    float4 position				: POSITION;
    float2 texCoordDiffuse		: TEXCOORD0;
    float4 color				: COLOR0;
};

struct vertexOutput {
    float4 Position			: POSITION;
    float2 texCoordDiffuse	: TEXCOORD0;
    float4 color			: COLOR0;
};


//------------------------------------
vertexOutput VS_TransformAndTexture(vertexInput IN) 
{

    vertexOutput OUT;
    OUT.texCoordDiffuse = IN.texCoordDiffuse;
    
    float4x4 wvp = mul(world, viewProjection);
    
    
    OUT.Position = mul(IN.position,wvp);
	  OUT.color=IN.color;
    
    return OUT;
}


//------------------------------------
sampler TextureSampler = sampler_state 
{
    texture = <diffuseTexture>;
    AddressU  = CLAMP;        
    AddressV  = CLAMP;
    AddressW  = CLAMP;
    MIPFILTER = LINEAR;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
};


//-----------------------------------
float4 PS_Textured( vertexOutput IN): COLOR
{
  float4 diffuseTexture = tex2D( TextureSampler, IN.texCoordDiffuse );
  return diffuseTexture;// * IN.color;
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
