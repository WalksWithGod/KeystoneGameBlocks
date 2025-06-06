//------------------------------------
float4x4 worldViewProj : WorldViewProjection;
texture diffuseTexture : Diffuse;


struct vertexInput {
    float4 position				: POSITION;
    float3 texCoordDiffuse		: TEXCOORD0;
};

struct vertexOutput {
    float4 hPosition		: POSITION;
    float3 texCoordDiffuse	: TEXCOORD0;
};


//------------------------------------
vertexOutput VS_TransformAndTexture(vertexInput IN) 
{
    vertexOutput OUT;
    OUT.hPosition = mul(IN.position, worldViewProj);
    OUT.texCoordDiffuse = IN.texCoordDiffuse;
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
  float4 diffuseTexture = texCUBE( TextureSampler, IN.texCoordDiffuse );
  
  return diffuseTexture;
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