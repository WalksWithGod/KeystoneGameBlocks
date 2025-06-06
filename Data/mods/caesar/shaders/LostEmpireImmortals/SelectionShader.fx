float4x4 viewProjection : viewProjection;
texture diffuseTexture : Diffuse;
float selectionAlpha;

//------------------------------------
struct vertexInput {
    float4 position				: POSITION;
    float4 texCoordDiffuse		: TEXCOORD0;
    float4 color				: COLOR0;
};

struct vertexOutput {
    float4 hPosition		: POSITION;
    float4 texCoordDiffuse	: TEXCOORD0;
    float4 color			: COLOR0;
};


//------------------------------------
vertexOutput VS_TransformAndTexture(vertexInput IN) 
{
    vertexOutput OUT;
    OUT.hPosition = mul(IN.position , viewProjection);
    OUT.texCoordDiffuse = IN.texCoordDiffuse;
    OUT.color=IN.color;
    return OUT;
}


//------------------------------------
sampler TextureSampler = sampler_state 
{
    texture = <diffuseTexture>;
    AddressU  = ClAMP;        
    AddressV  = CLAMP;
    AddressW  = CLAMP;
    MIPFILTER = LINEAR;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
};


//-----------------------------------
float4 PS_Textured( vertexOutput IN): COLOR
{
  float4 dTexture = tex2D( TextureSampler, IN.texCoordDiffuse );
  return dTexture * IN.color * selectionAlpha;
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