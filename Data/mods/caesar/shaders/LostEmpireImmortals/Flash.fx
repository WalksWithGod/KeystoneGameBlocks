//------------------------------------
texture diffuseTexture : Diffuse;
float globalAlpha;

//------------------------------------
struct vertexInput {
    float4 position				: POSITION;
};

struct vertexOutput {
    float4 position		: POSITION;
};

//------------------------------------
vertexOutput Transform(vertexInput IN) 
{
    vertexOutput OUT;
    
    OUT.position=IN.position;

    return OUT;
}

//------------------------------------
sampler TextureSampler = sampler_state 
{
    texture = <diffuseTexture>;
    AddressU  = WRAP;        
    AddressV  = WRAP;
    AddressW  = WRAP;
    MIPFILTER = LINEAR;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
};

float4 PixelShader(vertexOutput IN): COLOR
{
  return globalAlpha;
}

technique TransformTechnique
{
    pass P0
    {
        vertexShader = compile vs_1_1 Transform();
        pixelShader = compile ps_1_1 PixelShader();
    }
}