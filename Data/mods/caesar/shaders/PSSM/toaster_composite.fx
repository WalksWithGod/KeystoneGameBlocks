texture Color;
ntexture Shadow1;
ntexture Shadow2;
ntexture Shadow3;

nsampler colorSampler = sampler_state
{
    Texture = (Color);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    Mipfilter = LINEAR;
};

nsampler shadow1Sampler = sampler_state
{
    Texture = (Shadow1);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    Mipfilter = LINEAR;
};

nsampler shadow2Sampler = sampler_state
{
    Texture = (Shadow2);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    Mipfilter = LINEAR;
};

sampler shadow3Sampler = sampler_state
{
    Texture = (Shadow3);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    Mipfilter = LINEAR;
};

struct VertexShaderInput
{
    float3 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
};
struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
};

float2 halfPixel;
VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
    output.Position = float4(input.Position,1);
    output.TexCoord = input.TexCoord - halfPixel;
    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float3 diffuseColor = tex2D(colorSampler, input.TexCoord).rgb;
    float3 shadow1 = tex2D(shadow1Sampler, input.TexCoord).rgb;
	float3 shadow2 = tex2D(shadow2Sampler, input.TexCoord).rgb;
	float3 shadow3 = tex2D(shadow3Sampler, input.TexCoord).rgb;
    //float3 finalShadow = max(shadow1, shadow2);
    return float4(diffuseColor * shadow1, 1);
}
technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
		//AlphaBlendEnable = false;
    }
}