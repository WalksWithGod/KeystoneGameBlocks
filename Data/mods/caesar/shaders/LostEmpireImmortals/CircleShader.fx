string description = "Basic Vertex Lighting with a Texture";

//------------------------------------
float4x4 world   : World;
float4x4 viewProjection : viewProjection;

texture circleTexture : Diffuse
<
	string ResourceName = "default_color.dds";
>;

float4 circleColor : Color
<
> = {1.0f, 1.0f, 1.0f, 1.0f};

float circleRadius = 1.0f;




//------------------------------------
struct vertexInput {
    float4 position				: POSITION;
    float4 texCoordDiffuse		: TEXCOORD0;
    float4 direction			: TEXCOORD1;
};

struct vertexOutput {
    float4 hPosition		: POSITION;
    float4 texCoordDiffuse	: TEXCOORD0;
};

struct stencilInput {
	float4 position				: POSITION;
};

struct stencilOutput {
	float4 position				: POSITION;
};




//------------------------------------
vertexOutput VS_TransformAndTexture(vertexInput IN) 
{
    vertexOutput OUT;
    float4x4 worldViewProj = mul(world, viewProjection);
    
    float4 hPosition = IN.position + IN.direction * float4(circleRadius,0,circleRadius,0);
    
    OUT.hPosition = mul( hPosition , worldViewProj);
    OUT.texCoordDiffuse = IN.texCoordDiffuse;
    return OUT;
}

stencilOutput VS_Stencil(stencilInput IN)
{
	stencilOutput OUT;
	float4x4 worldViewProj = mul(world, viewProjection);
	
	float4 position = IN.position*float4(circleRadius,0,circleRadius,1.0f);
	OUT.position = mul(position, worldViewProj);
	return OUT;
}




//------------------------------------
sampler TextureSampler = sampler_state 
{
    texture = <circleTexture>;
    AddressU  = WRAP;        
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
  return circleColor * diffuseTexture;
}

float4 PS_Stencil(stencilOutput IN) : COLOR
{
	return float4(1,1,1,1);
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

technique stencil
{
	pass p0
	{
		VertexShader = compile vs_1_1 VS_Stencil();
		PixelShader = compile ps_1_1 PS_Stencil();
	}
}