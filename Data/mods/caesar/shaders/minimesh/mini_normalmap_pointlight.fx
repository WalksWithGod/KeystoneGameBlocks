float4x3 World[52] : MINIMESH_WORLD;
float4x4 ViewProj  : VIEWPROJECTION;
float4   Colour[52]: MINIMESH_COLOR;
float3   cp        : VIEWPOS;
texture  colour    : TEXTURE0;
texture  bump      : TEXTURE1;

// LIGHTPOINTx_ semantics broken in minimesh shaders?
float3 LightPos ; //: LIGHTPOINT0_POSITION ;
float  Radius ; // : LIGHTPOINT0_RANGE ;
// float3 LightColor : LIGHTPOINT0_COLOR; // Why isn't light color used?  I dont see any semantic for it?

sampler2D cs = sampler_state
{
	 Texture   = (colour);
	 MIPFILTER = Anisotropic;
	 MAGFILTER = Anisotropic;
	 MINFILTER = Anisotropic;
};

sampler2D bs = sampler_state
{
	 Texture   = (bump);
	 MIPFILTER = Anisotropic;
	 MAGFILTER = Anisotropic;
	 MINFILTER = Anisotropic;
};

struct VertexIN
{
	 float4 Position: POSITION;
	 float3 t       : TANGENT;
	 float3 b       : BINORMAL;
	 float3 n       : NORMAL;
	 float2 UV      : TEXCOORD0;
	 float2 Index   : TEXCOORD3;
};

struct FragmentIN
{
	 float4 Position: POSITION;
	 float2 UV      : TEXCOORD0;
	 float3 LightVec: TEXCOORD1;
	 float3 vv      : TEXCOORD2;
	 float  At      : TEXCOORD3;
	
	 float4 Colour  : COLOR0;
};

// Lyon Specular
float CalculateLyon(float3 VV, float3 LV, float3 NV)
{
	 float3 HalfWay     = normalize(VV + LV);
	 float3 Difference  = HalfWay - NV;
	 float  SS          = saturate(dot(Difference, Difference) * 60);
	 return pow(1 - SS, 3);
}

void VertexProgram(in VertexIN IN, out FragmentIN OUT)
{
	 float4x3 WorldMatrix  = World[IN.Index.x];
	 float3   WorldPos     = mul(IN.Position, WorldMatrix);
	          OUT.Position = mul(float4(WorldPos, 1), ViewProj);
	          OUT.UV       = IN.UV;
	          OUT.Colour   = Colour[IN.Index.x];

	 float3x3 TBN          = float3x3(IN.t, IN.b, IN.n);
	 float3x3 WTTS         = mul(TBN, WorldMatrix);

	          OUT.LightVec = mul(WTTS, (LightPos - WorldPos));
	          OUT.vv       = mul(WTTS, cp - WorldPos);
	          OUT.At       = distance(WorldPos, LightPos);
}

float4 FragmentProgram(in FragmentIN IN): COLOR0
{
	 float3 	viewVec = normalize(IN.vv);
	 float2	 nuv     = (tex2D(bs, IN.UV).a * 0.20f - (0.05f * 0.5f)) * viewVec + IN.UV;
	 
	 float3  np      = 2 * tex2D(bs, nuv) - 1;
	 float3  ld      = normalize(IN.LightVec);
	 float   d       = saturate(dot(np, ld) * 0.5f + 0.5f) * clamp(1 - length(IN.At) / Radius, 0, 1);
	 float   s       = s = 0; // CalculateLyon(viewVec, ld, np); // s = 0; // for specular disabled

	 float4  Pixel   = (d * float4(tex2D(cs, nuv).rgb, 1) + s) * IN.Colour;
	 return  Pixel;
}

technique render
{
	 pass pass0
	 {
		 VertexShader     = compile vs_3_0 VertexProgram();
		 PixelShader      = compile ps_3_0 FragmentProgram();
	 }
}
