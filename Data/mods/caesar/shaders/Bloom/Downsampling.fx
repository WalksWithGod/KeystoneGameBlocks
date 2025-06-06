// -------------------------------------------------------------
// Texture & Sampler
// -------------------------------------------------------------
texture texTexture : TEXTURE0;
sampler sampTexture = sampler_state {
	Texture = (texTexture);
    AddressU = Clamp;
    AddressV = Clamp;
};

// -------------------------------------------------------------
// Parameter
// -------------------------------------------------------------
const float2 texelSize;
// -------------------------------------------------------------
// Input/Output channels
// -------------------------------------------------------------
struct VS_INPUT
{
	float4 position : POSITION;
	float2 texCoord : TEXCOORD0;
};
struct VS_OUTPUT
{
	float4 position : POSITION;
	float2 taps[4] : TEXCOORD0;
};
#define PS_INPUT VS_OUTPUT

// -------------------------------------------------------------
// Vertex Shader
// -------------------------------------------------------------
VS_OUTPUT VS(const VS_INPUT IN)
{
	VS_OUTPUT OUT;
	
	OUT.position = IN.position;
	
	OUT.taps[0] = IN.texCoord - texelSize / 4;
	OUT.taps[1] = IN.texCoord + float2(-texelSize.x, texelSize.y) / 4;
	OUT.taps[2] = IN.texCoord + float2(texelSize.x, -texelSize.y) / 4;
	OUT.taps[3] = IN.texCoord + texelSize / 4;
	
	return OUT;
}

// -------------------------------------------------------------
// Pixel Shaders
// -------------------------------------------------------------
float4 PS(const PS_INPUT IN) : COLOR
{
	float4x4 samples;
	for(int i=0; i<4; i++)
		samples[i] = tex2D(sampTexture, IN.taps[i]);

	return mul(0.25f.xxxx, samples);
}

// -------------------------------------------------------------
// Techniques
// -------------------------------------------------------------
technique TSM3
{
	pass P
	{
		VertexShader = compile vs_3_0 VS();
		PixelShader = compile ps_3_0 PS();
	}
}
technique TSM2a
{
	pass P
	{
		VertexShader = compile vs_2_0 VS();
		PixelShader = compile ps_2_a PS();
	}
}
technique TSM2b
{
	pass P
	{
		VertexShader = compile vs_2_0 VS();
		PixelShader = compile ps_2_b PS();
	}
}
technique TSM2
{
	pass P
	{
		VertexShader = compile vs_2_0 VS();
		PixelShader = compile ps_2_0 PS();
	}
}