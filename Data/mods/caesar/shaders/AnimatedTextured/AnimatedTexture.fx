// -------------------------------------------------------------
// Semantics
// -------------------------------------------------------------
float4x4 matWorldViewProj : WORLDVIEWPROJECTION;
float4x4 matWorld : WORLD;	
float4x4 matWorldIT : WORLDINVERSETRANSPOSE;


// -------------------------------------------------------------
// Parameters
// -------------------------------------------------------------
uniform extern float ticks : TICKCOUNT;

// contains u,v offset and the width and height of the sprite
uniform extern float4 textureUVInfo = float4 (0.0, 0.0, 1.0, 1.0);  // 0 = u, 1 = v, 2 = width, 3 = height


// -------------------------------------------------------------
// Textures and Samplers
// -------------------------------------------------------------
Texture diffuseTexture : TEXTURE0;

sampler2D Diffuse = sampler_state
{
	texture = <diffuseTexture>;
	mipfilter = None;  // 2d billboards should not use mipmaps, only perspective renders should
	magfilter = Point;
	minfilter = Point;
	AddressU  = Clamp; 
	AddressV  = Clamp; 
};


// -------------------------------------------------------------
// Input/Output channels
// -------------------------------------------------------------
struct VertexToPixel
{
    float4 Position     : POSITION;
    float2 TexCoords    : TEXCOORD0;
    float3 Normal       : TEXCOORD1;
    float3 Position3D   : TEXCOORD2;
    float3 Orig_pos     : TEXCOORD3;   
};

struct PixelToFrame
{  
    float4 Color        : COLOR0;
};

// -------------------------------------------------------------
// Vertex Shader function
// -------------------------------------------------------------
VertexToPixel VS( 
		float4 inPos : POSITION0, 
		float3 inNormal: NORMAL0, 
		float2 inTexCoords : TEXCOORD0, 
		float4 Color : COLOR0)
{

    // this is a standard vertex shader I use in lots of effects, so it passes more stuff than this effect needs
    VertexToPixel Output = (VertexToPixel)0;
 
    Output.Position   = mul(inPos, matWorldViewProj);
 
    Output.Normal     = normalize(mul(inNormal, (float3x3) matWorldIT));
    
    Output.Position3D = mul(inPos, matWorld);
 	Output.Orig_pos = inPos;
    Output.TexCoords  = inTexCoords;
    return Output;
};

// -------------------------------------------------------------
// Pixel Shader function
// -------------------------------------------------------------
PixelToFrame PS (VertexToPixel IN)
{
	PixelToFrame Output = (PixelToFrame)0;
	
	float4 color;
	float2 uv;
	float halfTexel = 0.5 / 256.0; // http://drilian.com/2008/11/25/understanding-half-pixel-and-half-texel-offsets/
	uv[0] = (IN.TexCoords.x * textureUVInfo.z) + textureUVInfo.x + halfTexel;
	uv[1] = (IN.TexCoords.y * textureUVInfo.w) + textureUVInfo.y + halfTexel;
	
	//uv[0] = IN.TexCoords.x;
	//uv[1] = IN.TexCoords.y;
	
    color = tex2D(Diffuse, uv);

    Output.Color =color;
 
	return Output;
};

// -------------------------------------------------------------
// Technique
// -------------------------------------------------------------
technique primary
{
    pass pass0
    {
         VertexShader = compile vs_2_0 VS();
         PixelShader = compile ps_2_0 PS();
    }
}
