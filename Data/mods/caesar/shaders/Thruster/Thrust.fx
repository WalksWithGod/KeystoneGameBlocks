//http://www.youtube.com/watch?v=hBBMgM3oLcw
//I keep meaning to make a tutorial.
// 
//There's a color map, alpha map (greyscale of color), 
// and an alpha gradient.
// 
//Plume is? a cylinder, UV mapped 'vertically'.
// 
//Color and alpha map increase their 'V' offset by different values 
//(say color 0.5 alpha 0.8) in the UV every frame. This creates moving 
//flames and 'holes' that move relative to the flames.
// 
//Alpha gradient stays put and caps it at the bottom (fade out). 
//Or if you slide the alpha gradient's 'V' +/- 1 you'll throttle 
//the engine up and down.


// -------------------------------------------------------------
// Semantics
// -------------------------------------------------------------
float4x4 matWorldViewProj 	: WORLDVIEWPROJECTION;
float4x4 matWorld 			: WORLD;	
float4x4 matWorldIT 		: WORLDINVERSETRANSPOSE;


// -------------------------------------------------------------
// Parameters
// -------------------------------------------------------------
float g_Ticks					: TICKCOUNT;

// the alpha value will contain a value between 
// 0 and 1 to represent the percentage of full thrust
float4 thrust_color[2] = { 	float4(1, 0, 0, 0.1f), // red
							float4(1, 1, 0, 1)     // yellow
						 };  


// -------------------------------------------------------------
// Textures and Samplers
// -------------------------------------------------------------
Texture noise_texture 		: TEXTURE1; // normalmap layer 1

sampler3D NoiseSampler = sampler_state
{
	texture = <noise_texture>;
	mipfilter = Linear;
	magfilter = Linear;
	minfilter = Linear;
	AddressU  = mirror; 
	AddressV  = mirror; 
};


// -------------------------------------------------------------
// Input/Output channels
// -------------------------------------------------------------
struct VS_INPUT 
{
	float4 PositionMS 		: POSITION;	
	float2 UV 				: TEXCOORD;		
	float3 Normal 			: NORMAL;			
};

struct VS_OUTPUT
{
    float4 PositionWS     	: POSITION;
    float2 TexCoords    	: TEXCOORD0;
    float3 Normal       	: TEXCOORD1;
    float3 Position3D   	: TEXCOORD2;
    float3 Orig_pos     	: TEXCOORD3;   
};


// -------------------------------------------------------------
// Vertex Shader function
// -------------------------------------------------------------

VS_OUTPUT VS( VS_INPUT IN)
{
    VS_OUTPUT OUT = (VS_OUTPUT)0;
 
    OUT.PositionCS = mul(IN.PositionMS, matWorldViewProj);
    OUT.PositionWS = mul(IN.PositionMS, matWorld);
    OUT.Normal     = normalize(mul(IN.Normal, (float3x3) matWorldIT));
 	OUT.PositionMS = IN.PositionMS;
    OUT.TexCoords  = IN.UV;
	
    return OUT;
};

// -------------------------------------------------------------
// Pixel Shader function
// -------------------------------------------------------------
float4 PS (VS_OUTPUT IN) : COLOR 
{
    float cosang;
    float edgeAlpha;
    float4 color;
    float4 up = normalize(matWorld[1]);
	
    float noiseval;

	// Hypno - length of the exhaust plume
    float heatfactor = thrust_color[0].a;

	// Hypno - use the distance from origin to affect "heat" color and alpha
	//         This means the mesh origin is center of "top" cap of cone/cylinder.
    float heat = clamp((1 - IN.PositionMS.z / 2) ,0,1);

    noiseval = tex3D(NoiseSampler,float3(IN.TexCoords.x + g_Ticks / 25,
	    								IN.TexCoords.y - g_Ticks * ( 5 + 20 * heatfactor), 
	 									g_Ticks));

    noiseval = pow(noiseval, 1.5f + 1.5f * heatfactor);

	color.rgb = (2.5f * noiseval * thrust_color[1].rgb + 
				 10 * noiseval * pow(heat * heatfactor,3.5f) * thrust_color[0].rgb); 

	// the closer the normal is to Up vector, the more transparent. on a curved mesh, this has the effect
	// of making outward from center line more transparent... but i dont understand how this also
	// tapers the end into a cone?  I think this IN.Normal since it's transformed by matWorldIT is
	// actually the TANGENT SPACE normal
    edgeAlpha = 1 - abs(dot(up, IN.Normal));

	color.a =  edgeAlpha * pow(heat, .85f + 25 * (1 - heatfactor));
    return color;
};

// -------------------------------------------------------------
// Technique
// -------------------------------------------------------------
technique technique0
{
    pass pass0
    {
		ZWRITEENABLE = FALSE;
        VertexShader = compile vs_3_0 VS();
        PixelShader = compile ps_3_0 PS();
    }
}
