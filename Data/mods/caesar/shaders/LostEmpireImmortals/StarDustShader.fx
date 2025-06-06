string description = "Planet Shader";

//------------------------------------
float4x4 view : View;
float4x4 projection : Projection;
float rotation;
float elapsedTime;
float globalScale;
float colorMul;

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
    
    //Move the position into camera space
    
    float4 position = mul(IN.position,view);
	OUT.color=IN.color*colorMul;
	//OUT.color.a = position.z*0.003f;
	//Displace corner	
	                
	float2 move;
	
	float coss=sin(IN.offset.x+rotation)*IN.offset.y;
	float sinn=cos(IN.offset.x+rotation)*IN.offset.y;
	
	move.x=coss - sinn;
	move.y=sinn + coss;
			
	position.xy += move*globalScale;
	
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
  diffuseTexture.rgb = diffuseTexture.rgb * IN.color.rgb;
  //diffuseTexture.a = IN.color.a;
  return diffuseTexture * IN.color.a;
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
