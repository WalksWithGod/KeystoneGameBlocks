//----- Constants ---------

float4x4 world : WORLD;
float4x4 viewProj : VIEWPROJ;

float3 lightPos;

//----- Shaders -----------

struct vsOut {
    float4 pos: POSITION;
    float depth: TEXCOORD0;
};

//outputs depth value in world space
void VS(in float3 pos: POSITION, out vsOut output) {
    //transform vertex to world space
    output.pos = mul(float4(pos, 1.0), world);
    //save depth
    output.depth = length(output.pos.xyz - lightPos.xyz);
    //transform to projection space
    output.pos = mul(output.pos, viewProj);
}

float4 PS(in float depth: TEXCOORD0) : COLOR {
    //output depth and depth squared (for variance shadow mapping)
    return float4(depth, depth*depth, 0.0, 1.0);
}

technique main
{
    pass p0 
    {		
		VertexShader = compile vs_1_1 VS();
		PixelShader  = compile ps_2_0 PS();
    }
}