
// -------------------------------------------------------------
// Semantics
// -------------------------------------------------------------
float4x4 World 			: WORLD;	
float4x4 View 			: VIEW;
float4x4 Projection 	: PROJECTION;

// -------------------------------------------------------------
// Parameters
// -------------------------------------------------------------
float gInnerLimit = 1.0;
float gOuterLimit = 1.5;
float gRadius = 20.0;
float gTransparency = 0.15;


// -------------------------------------------------------------
// Textures and Samplers
// -------------------------------------------------------------
texture diskTex : TEXTURE0;
sampler DiskSampler = sampler_state 
{
	texture = <diskTex>;
	MagFilter = Anisotropic;
	MinFilter = Anisotropic;
	MipFilter = Anisotropic;
	//magfilter	= LINEAR; 
	//minfilter	= LINEAR; 
	//mipfilter	= LINEAR; 
	AddressU	= CLAMP;  
	AddressV	= CLAMP;
};


// -------------------------------------------------------------
// Input/Output channels
// -------------------------------------------------------------
struct VertexShaderInput
{
    float4 Position 	: POSITION0;
    float2 TexCoord 	: TEXCOORD0;
    
};

struct VertexShaderOutput
{
    float4 Position 	: POSITION0;
	float2 TexCoord 	: TEXCOORD0;
	float3 PositionWS   : TEXCOORD1; 
	
};

// -------------------------------------------------------------
// Vertex Shader function
// -------------------------------------------------------------
VertexShaderOutput VertexShaderFunction(VertexShaderInput IN)
{
    VertexShaderOutput OUT;

    float4 worldPosition = mul(IN.Position, World);
    float4 viewPosition = mul(worldPosition, View);
	
    OUT.Position = mul(viewPosition, Projection); 
	OUT.TexCoord = IN.TexCoord;
    OUT.PositionWS = worldPosition.xyz;
	
    return OUT; // output gets interpolated when arriving in the PixelShader
}

// -------------------------------------------------------------
// Pixel Shader function
// -------------------------------------------------------------
float4 PixelShaderFunction(VertexShaderOutput IN) : COLOR0
{
	float2 uv;
	float3 center = float3(World[3][0], World[3][1], World[3][2]);
	float l = distance(IN.PositionWS, center);
	
	// color pixel based on the range in world coords away from center of disk mesh
	if (l < gInnerLimit)
	{
		uv = float2(0.3, 0.5);
	}
	else if (l > gOuterLimit)
	{
		uv = float2(0.9, 0.5);
	}
	else uv = float2(0.6, 0.5);
		
	// TODO: I don't even need to use a texture, i can just hardcode our
	//       color values. for now leave it til i fix other bugs.
	float4 result = tex2D(DiskSampler, uv);
	result.w = gTransparency;
	
    return result;
}

// -------------------------------------------------------------
// Technique
// -------------------------------------------------------------
technique primary
{
    pass pass0
    {
		// note: its best to not include state changes in the technique since your app code should do that
		// and then you can reduce state changes. This seems to be the concensus of gamedev.net folks. 
		// All we need to do is set  mesh.BlendingMode = CONST_TV_BLENDINGMODE.TV_BLEND_ALPHA and it works 
		// as well as setting renderstates directly in the shader.
		// note: do not uncomment.  
		//AlphaBlendEnable=true;
		//SrcBlend=srcalpha;
		//DestBlend=invsrcColor;
	
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
