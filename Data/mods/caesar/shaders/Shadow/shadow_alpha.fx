//----- Constants ---------

float4x4 world : WORLD;
float4x4 viewProj : VIEWPROJ;

float3 lightPos;

texture DiffuseMap 	: TEXTURE0;
sampler DiffuseMapSampler = sampler_state {

    Texture = <DiffuseMap>;

};

//----- Shaders -----------

struct vsOut {
    	float4 pos: POSITION;
   	float depth: TEXCOORD0;
	float2 texCoord : TEXCOORD1;
};

//outputs depth value in world space
void VS(in float3 pos: POSITION, float2 texCoord: TEXCOORD0, out vsOut output) {

    	output.pos = mul(float4(pos, 1.0), world);
    	output.depth = length(output.pos.xyz - lightPos.xyz);
   	output.pos = mul(output.pos, viewProj);

	output.texCoord = texCoord;

}

float4 PS(in float depth: TEXCOORD0, float2 texCoord: TEXCOORD1) : COLOR {
    
	clip(tex2D(DiffuseMapSampler, texCoord).a - 0.5f);

    	return float4(depth, 0.0, 0.0, 1.0);
}

technique main
{
    pass p0 
    {		
		VertexShader = compile vs_1_1 VS();
		PixelShader  = compile ps_2_0 PS();
    }
}