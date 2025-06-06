string description = "Planet Shader";

//------------------------------------
float4x4 view : View;
float4x4 projection : Projection;

texture diffuseTexture : Diffuse
<
	string ResourceName = "default_color.dds";
>;

//------------------------------------
struct vertexInput {
    float4 position				: POSITION;
    float2 texCoordDiffuse		: TEXCOORD0;
    float2 offset				: TEXCOORD1;
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
    float4 position = mul(IN.position,view);
	OUT.color=IN.color;
	                
	float2 move;
	
	float coss=sin(IN.offset.x)*IN.offset.y;
	float sinn=cos(IN.offset.x)*IN.offset.y;
	
	move.x=coss - sinn;
	move.y=sinn + coss;
			
	position.xy += move;
	
	OUT.Position = mul(position, projection);
    
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
  return diffuseTexture * IN.color;
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
