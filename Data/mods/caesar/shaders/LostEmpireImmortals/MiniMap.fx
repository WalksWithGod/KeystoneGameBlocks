texture Texture : Diffuse;
float2 Scales;
float2 Offset;

struct VS_OUTPUT
{
    float4 position : POSITION;
    float2 texcoord : TEXCOORD0;
    float4 color : COLOR0;
    float2 offset : TEXCOORD1;
};

VS_OUTPUT Transform(
    float4 Pos  : POSITION,
    float2 texcoord : TEXCOORD0, 
    float4 Color : COLOR0,
    float2 offset : TEXCOORD1
    )
{
    VS_OUTPUT Out = (VS_OUTPUT)0;

    Out.position = Pos;
    
    float4 off = float4(-Offset.x+offset.x/Scales.x,-Offset.y+offset.y/Scales.y,0,0);    
    Out.position += off;
        
    float4 scale = float4(Scales.x, Scales.y,1,1);
    Out.position *= scale;
	
	
    
    Out.texcoord = texcoord;
        
    Out.color = Color;

    return Out;
}

sampler TextureSampler = sampler_state 
{
    texture = <Texture>;
    AddressU  = CLAMP;        
    AddressV  = CLAMP;
    AddressW  = CLAMP;
    MIPFILTER = LINEAR;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
};


//-----------------------------------
float4 PixelShaderDiffuse(VS_OUTPUT IN): COLOR
{
  float4 diffuseTexture = tex2D( TextureSampler, IN.texcoord );
  return diffuseTexture * IN.color;
}

float4 PixelShaderWhite(): COLOR
{
  return 1;
}


technique TransformTechnique
{
    pass P0
    {
        vertexShader = compile vs_1_1 Transform();
        pixelShader = compile ps_1_1 PixelShaderDiffuse();
    }
}

technique TransformTechniqueWhite
{
    pass P0
    {
        vertexShader = compile vs_1_1 Transform();
        pixelShader = compile ps_1_1 PixelShaderWhite();
    }
}