string description = "Trade Route Shader";

//------------------------------------
float4x4 viewProjection : ViewProjection;
//float4x4 world : World;

float time : Time;

texture diffuseTexture : Diffuse
<
	string ResourceName = "default_color.dds";
>;

//------------------------------------
struct vertexInput {
    float4 position				: POSITION;
    float2 texCoordDiffuse1		: TEXCOORD0;
    float2 texCoordDiffuse2		: TEXCOORD1;
};

struct vertexOutput {
    float4 Position			: POSITION;
    float2 texCoordDiffuse1	: TEXCOORD0;
    float2 texCoordDiffuse2 : TEXCOORD1;
};


//------------------------------------
vertexOutput VS_TransformAndTexture(vertexInput IN) 
{

    vertexOutput OUT;
    OUT.texCoordDiffuse1 = IN.texCoordDiffuse1;
    OUT.texCoordDiffuse1.x += time;
    
    OUT.texCoordDiffuse2 = IN.texCoordDiffuse2;
    OUT.texCoordDiffuse2.x += time;
    
    //float4x4 wvp = mul(world, viewProjection);
    
    
    OUT.Position = mul(IN.position,viewProjection);
    
    return OUT;
}


//------------------------------------
sampler TextureSampler = sampler_state 
{
    texture = <diffuseTexture>;
    AddressU  = Wrap;        
    AddressV  = Wrap;
    AddressW  = Wrap;
    MIPFILTER = LINEAR;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
};


//-----------------------------------
float4 PS_Textured( vertexOutput IN): COLOR
{
  float4 diffuseTexture1 = tex2D( TextureSampler, IN.texCoordDiffuse1 );
  float4 diffuseTexture2 = tex2D( TextureSampler, IN.texCoordDiffuse2 );
  return diffuseTexture1 + diffuseTexture2;// * IN.color;
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
