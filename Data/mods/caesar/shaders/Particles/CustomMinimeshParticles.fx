// -------------------------------------------------------------
/// Billboard Particle Minimesh Shader - Textured + Colors with Alpha Per Instance + Texture Rotation
// -------------------------------------------------------------

// -------------------------------------------------------------
// FOG CONSTANTS
// -------------------------------------------------------------
#define FOG_TYPE_NONE    0
#define FOG_TYPE_EXP     1
#define FOG_TYPE_EXP2    2
#define FOG_TYPE_LINEAR  3 

// -------------------------------------------------------------
// MATRIX SEMANTICS
// -------------------------------------------------------------
float3 g_ViewPosition			: VIEWPOSITION;
float4x4 g_World              	: WORLD;
float4x4 ViewProjection        	: VIEWPROJECTION;
float4x3 minimeshes[52]     	: MINIMESH_WORLD;
float4 minimeshes_color[52] 	: MINIMESH_COLOR;

// -------------------------------------------------------------
// FOG SEMANTICS
// -------------------------------------------------------------
int fogType 					: FOGTYPE;
float3 fogColor 				: FOGCOLOR; 
float fogDensity 				: FOGDENSITY;
float fogStart 					: FOGSTART;
float fogEnd 					: FOGEND;

// -------------------------------------------------------------
// TEXTURES & SAMPLERS
// -------------------------------------------------------------
texture textureDiffuse          : TEXTURE0;
sampler diffuseSampler = sampler_state
{
    Texture = <textureDiffuse>;
    
    MinFilter = LINEAR;
    MagFilter = LINEAR;
    MipFilter = POINT;
   
    AddressU = CLAMP;
    AddressV = CLAMP;
};
// -------------------------------------------------------------
// INPUT / OUTPUT STRUCTS
// -------------------------------------------------------------
struct VS_INPUT
{
    float4 PositionMS	: POSITION;
    float3 normal  		: NORMAL;     // normal is only relevant for NON-Billboard miminesh elements
	                                  // otherwise we can fill the normal with 2d texture rotation value
    float2 UV     		: TEXCOORD0;
    float2 index   		: TEXCOORD3;  // Minimesh Element Index is ALWAYS on TEXCOORD3.
};

struct VS_OUTPUT
{
    float4 PositionCS	: POSITION;
    float2 UV     		: TEXCOORD0;
    float4 Color   		: COLOR0;
};

// -------------------------------------------------------------
// VERTEX SHADER
// -------------------------------------------------------------
VS_OUTPUT VS_MiniMesh(VS_INPUT IN)
{
	VS_OUTPUT OUT;
	float4x3 worldMatrix  = minimeshes[IN.index.x];
	
	float3 positionWS     = mul(IN.PositionMS, worldMatrix);
	OUT.PositionCS        = mul(float4(positionWS, 1.0f), ViewProjection);
	OUT.UV                = IN.UV;
	OUT.Color 			  = minimeshes_color[IN.index.x];
	return OUT;
} 

// -------------------------------------------------------------
// PIXEL SHADER
// -------------------------------------------------------------
float4 PS_MiniMesh(VS_OUTPUT IN) : COLOR0
{
	return tex2D(diffuseSampler, IN.UV)* IN.Color;

//	// texture rotation for Billboard Minimesh elements Only
//	float r = (IN.Rotation * 6.283185f) - 3.141593f;
//	
//	float c = cos(r);
//	float s = sin(r);
//	
//	float2x2 rotationMatrix = float2x2(c, -s, s, c);
//	
//	float2 texCoord = mul(IN.UV - 0.5f, rotationMatrix);
//	
//	return tex2D(diffuseSampler, texCoord + 0.5f) * IN.Color;
}

// -------------------------------------------------------------
// TECHNIQUES
// -------------------------------------------------------------
technique minimesh
{
	pass pass0
	{
		//CULLMODE = CCW;
		//ALPHABLENDENABLE = TRUE;
		//SRCBLEND = SRCALPHA;
      	//DESTBLEND = INVSRCALPHA;
		//ALPHATESTENABLE = FALSE;
		//BLENDFUNCTION = ADD;
		
		VertexShader = compile vs_3_0 VS_MiniMesh();
		PixelShader = compile ps_3_0 PS_MiniMesh();
	}
} 