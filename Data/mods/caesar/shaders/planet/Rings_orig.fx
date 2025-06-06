float4x4 matWorldViewProj : WORLDVIEWPROJECTION;
float4x4 World : WORLD;	
float4x4 View : VIEW;
float4x4 Projection : PROJECTION;
float3 LightPosition : LIGHTPOINT0_POSITION;
float3 CameraPosition : CAMERAPOSITION;
float scale2 = 1000000;			//radius of planet squared // Hypno todo: this i need to update

texture ringtex  : TEXTURE0;
sampler RingSampler = sampler_state 
{
    texture = <ringtex>;
    magfilter	= LINEAR; 
	minfilter	= LINEAR; 
	mipfilter	= LINEAR; 
	AddressU	= CLAMP;  
	AddressV	= CLAMP;
};

texture noisetex  : TEXTURE1;
sampler NoiseSampler = sampler_state 
{
    texture = <noisetex>;
    magfilter	= LINEAR; 
	minfilter	= LINEAR; 
	mipfilter	= LINEAR; 
	AddressU	= CLAMP;  
	AddressV	= CLAMP;
};


struct VertexShaderInput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
    
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
	float3 World    : TEXCOORD1; 
	
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    //float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(input.Position, matWorldViewProj); // mul(viewPosition, Projection);   
    output.TexCoord= input.TexCoord;
    output.World=worldPosition.xyz;

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float4 noise=float4(0,0,0,0);
	
	// work out how far from the centre of the planet a pixel is, and then use that to lookup the colour.
	float2 tc = input.TexCoord;
	tc =(tc-0.5) * 0.5;
	tc.x=length(tc)*4;
	tc.y=0;
    	
	float4 result = tex2D(RingSampler,tc);
	result.w*=0.5;

	// sample in some noise to make the rings less solid 
	float3 depth = CameraPosition-input.World;
	float blend=length(depth);
	
	if (blend<100)
	{
		blend *= 0.1;
		noise=(tex2D(NoiseSampler,input.TexCoord));
	}else{
		blend=1;
	}
	
	//  blending
    	float3 dst = LightPosition;
    	float3 Ray = LightPosition-input.World;
    	float lr = length(Ray);
    	float ld = length(dst);
    	Ray=normalize(Ray);
	float B = dot(dst,Ray);
	float C = dot(dst, dst) - scale2;
	float D = B*B - C;
	if (D <=0) 
	{
		result.xyz = lerp(noise.xyz,result.xyz,blend);
		return result;
	}
	if (lr<ld)
	{
		result.xyz=0;
	}
	
	result.xyz = lerp(noise.xyz,result.xyz,blend);
    return result;
}

technique Technique1
{
    pass Pass1
    {
        // TODO: set renderstates here.
        VertexShader = compile vs_1_1 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
