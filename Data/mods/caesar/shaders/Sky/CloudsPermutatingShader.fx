float4x4 WorldViewProj : WORLDVIEWPROJECTION;

#define ONE 0.00390625 
#define ONEHALF 0.001953125
// The numbers above are 1/256 and 0.5/256 for
// 1 texel width and 1/2 texel width respectively.
// Change accordingly if a texture size other than 256 is used.

float time;
float4 SunColor;
float numTiles = 8.0f;  // for fewer larger clouds, use ~8.  for many, smaller use 32
float CloudCover = -0.1;
float CloudSharpness = 0.25;


texture permTexture : TEXTURE0;
sampler permSampler = sampler_state
{
	Texture = <permTexture>;
	MinFilter = Linear;
	MipFilter = Linear;
	MagFilter = Linear;
	ADDRESSU = WRAP;
	ADDRESSV = WRAP;
};

struct VertexShaderInput
{
    float4 PositionOS : POSITION;
    float2 TexCoord : TEXCOORD;
};

struct VertexShaderOutput
{
    float4 PositionCS : POSITION;
    float2 TexCoord : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    output.PositionCS =  mul(input.PositionOS, WorldViewProj);
    output.TexCoord = (input.TexCoord * numTiles);

    return output;
}


float fade(float t) {
  // return t*t*(3.0-2.0*t);
  return t*t*t* (t * (t * 6.0 - 15.0) + 10.0);
}


float noise(float2 P)
{
  float2 Pi = ONE * floor(P) + ONEHALF;
  float2 Pf = frac(P);

  float2 grad00 = tex2D(permSampler, Pi).rg * 4.0 - 1.0;
  float n00 = dot(grad00, Pf);

  float2 grad10 = tex2D(permSampler, Pi + float2(ONE, 0.0)).rg * 4.0 - 1.0;
  float n10 = dot(grad10, Pf - float2(1.0, 0.0));

  float2 grad01 = tex2D(permSampler, Pi + float2(0.0, ONE)).rg * 4.0 - 1.0;
  float n01 = dot(grad01, Pf - float2(0.0, 1.0));

  float2 grad11 = tex2D(permSampler, Pi + float2(ONE, ONE)).rg * 4.0 - 1.0;
  float n11 = dot(grad11, Pf - float2(1.0, 1.0));

  float2 n_x = lerp(float2(n00, n01), float2(n10, n11), fade(Pf.x));

  float n_xy = lerp(n_x.x, n_x.y, fade(Pf.y));

  return n_xy;
}



float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{

	float n = noise(input.TexCoord + time);  
	float n2 = noise(input.TexCoord * 2 + time);
	float n3 = noise(input.TexCoord * 4 + time);
	float n4 = noise(input.TexCoord * 8 + time);
	
	float nFinal = n + (n2 / 2) + (n3 / 4) + (n4 / 8);
	
	float c = CloudCover - nFinal;
    if (c < 0) 
		c=0;
 
    float CloudDensity = 1.0 - pow(CloudSharpness,c);
    
    float4 retColor = CloudDensity;
    retColor *= SunColor;
    
	SunColor.w = CloudDensity;
	return SunColor;
	// return float4(SunColor, saturate((layerAlpha[0] + layerAlpha[1]) * transparencyFactor));
	
    return retColor;

}

technique technique0
{
    pass pass0
    {
		// DestBlend is for the destination color. It is the pixel that already exists in this location. 
		// The blending function we are going to use for the destination is INV_SRC_ALPHA which is the 
		// inverse of the source alpha. This equates to one minus the source alpha value. For example 
		// if the source alpha is 0.3 then the dest alpha would be 0.7 so we will combine 70% of the 
		// destination pixel color in the final combine.
		// SrcBlend is for the source pixel which will be the input texture color value in this tutorial. 
		// The function we are going to use is SRC_ALPHA which is the source color multiplied by its own 
		// alpha value. 
		AlphablendEnable = true;
		AlphaTestEnable = false;
		SrcBlend = SrcAlpha;
		DestBlend = InvSrcAlpha;
		
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
