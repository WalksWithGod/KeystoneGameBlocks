//NOTE: Cubic refraction shader. Written By Stephen Smithbower.


//////////////////////////////////////////////////////////////////
//DEFINES														//
//////////////////////////////////////////////////////////////////
float4x4 WORLDIT 	: WorldInverseTranspose;
float4x4 WVP 		: WorldViewProjection;
float4x4 WORLD 		: World;
float4x4 VIEWI 		: ViewInverse;


//////////////////////////////////////////////////////////////////
//CONSTANTS														//
//////////////////////////////////////////////////////////////////
const float4 colors[3] = {
    	{ 1, 0, 0, 0 },
    	{ 0, 1, 0, 0 },
    	{ 0, 0, 1, 0 },
	};
	

//////////////////////////////////////////////////////////////////
//TWEAKABLES													//
//////////////////////////////////////////////////////////////////
float3 etas 			= { 0.80, 0.82, 0.84 };	//We use this later on, these are the wavelength diffraction amounts (RGB)

float bumpStrength 		= 1.0f;	//Max = 1, strength of normal map


//////////////////////////////////////////////////////////////////
//SAMPLERS														//
//////////////////////////////////////////////////////////////////
texture EnvironmentMap;
samplerCUBE EnvironmentMapSampler = sampler_state
{
	Texture = <EnvironmentMap>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
};

Texture NormalMap : TEXTURE1;
sampler SampNormalMap = sampler_state {

	Texture = <NormalMap>;
	
};


//////////////////////////////////////////////////////////////////
//STRUCTS														//
//////////////////////////////////////////////////////////////////

struct vIN {
    float4 Position	: POSITION;
    float4 TexCoord	: TEXCOORD0;
    float3 tangent	: TANGENT;
    float3 normal	: NORMAL;
    float3 binormal	: BINORMAL;
};


struct vOUT {
    float4 HPosition: POSITION;
    float2 TexCoord	: TEXCOORD0;
    float3 WorldView: TEXCOORD2;
    float3 normal	: TEXCOORD3;
    float3 binormal	: TEXCOORD4;
    float3 tangent	: TEXCOORD5;
};


//////////////////////////////////////////////////////////////////
//VERTEX SHADER													//
//////////////////////////////////////////////////////////////////

vOUT mainVS(in vIN IN) {
    vOUT OUT;
    
    float3 Pw 		= mul(IN.Position, WORLD).xyz;
    
    OUT.TexCoord 	= IN.TexCoord;
    
    OUT.WorldView 	= VIEWI[3].xyz - Pw; //Funky way to get viewvec, but it works (normal way inverts view)
    OUT.HPosition 	= mul(IN.Position, WVP);

	OUT.tangent 	= IN.tangent;
	OUT.binormal 	= IN.binormal;
	OUT.normal 		= IN.normal;

    return OUT;
}


//////////////////////////////////////////////////////////////////
//PIXEL SHADER													//
//////////////////////////////////////////////////////////////////

float4 mainPS(in vOUT IN) : COLOR
{

float3 normal 		= (tex2D(SampNormalMap, IN.TexCoord) * 2 - 1);

//------------------------------------------------------------
float3 tan 			= normalize(IN.tangent);	//Must make sure we convert normal from tangent space to world space
float3 bino 		= normalize(IN.binormal);	//in order to match worldview. Took me 2 days to figure that out =(
float3 no 			= normalize(IN.normal);

float3x3 matInverse = transpose(float3x3(tan, bino, no));

	   normal 		= mul(mul(normal, matInverse), WORLD);
//------------------------------------------------------------

float3 N 			= normalize(normal)* bumpStrength;
float3 V 			= normalize(IN.WorldView);
    

float3 R 			= reflect(-V, N);	//Reflection vector
float4 reflColor 	= texCUBE(EnvironmentMapSampler, R);

float  fresnel 		= (1.0 - dot(V, N)) * 1.5;	//Normally fresnel is without multiplier, but I think it looks better

        
float4 transColor 	= 0;

float3 Tr 			= refract(-V, N, etas[0]);
	   transColor 	+= texCUBE(EnvironmentMapSampler, Tr) * colors[0];	//Sample env.map and grab the colour + refract
float3 Tg 			= refract(-V, N, etas[1]);
	   transColor 	+= texCUBE(EnvironmentMapSampler, Tg) * colors[1];
float3 Tb 			= refract(-V, N, etas[2]);
	   transColor 	+= texCUBE(EnvironmentMapSampler, Tb) * colors[2];

return 				lerp(transColor, reflColor, fresnel);

}


//////////////////////////////////////////////////////////////////
//TECHNIQUES													//
//////////////////////////////////////////////////////////////////
technique main {
	pass p0 {		
        VertexShader = compile vs_2_0 mainVS();
        PixelShader = compile ps_2_0 mainPS();
	}
}

