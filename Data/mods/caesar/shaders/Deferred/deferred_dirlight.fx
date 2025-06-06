float4x4 wvp: WORLDVIEWPROJECTION;
float4x4 w  : WORLD;

float invWidth;   //(1 / ScreenWidth).
float invHeight;  //(1 / ScreenHeight).
float3   lp;      //Light position.
float3   lc;      //Light colour.
float    lr;      //Light range.

texture  c;       //Colour buffer.
texture  p;       //Position buffer.
texture  n;       //Normal buffer.

//Colour sampler.
sampler2D cs = sampler_state
{
	Texture = (c);
};

//Position sampler.
sampler2D ps = sampler_state
{
	Texture = (p);
};

//Normal sampler.
sampler2D ns = sampler_state
{
	Texture = (n);
};

//Application -> Vertex Program.
struct a2v
{
//Vertex position.
	float4 p : POSITION0;
//Vertex uv.
	float2 uv: TEXCOORD0;
};

//Vertex Program -> Fragment Program.
struct v2f
{
//Vertex position.
	float4 p  : POSITION0;
//Vertex uv.
	float2 uv : TEXCOORD0;
//Vertex uv in projected screen space.
	float4 suv: TEXCOORD1;
};

void vp(in a2v a2vin, out v2f v2fout)
{
//Put vertex in world view projection space.
	v2fout.p       = mul(a2vin.p, wvp);
//Dump uv.
	v2fout.uv      = a2vin.uv;	
//Projected co-ords.
	v2fout.suv     = v2fout.p;
	v2fout.suv.xy  = float2(v2fout.suv.x * 0.5, -v2fout.suv.y * 0.5);
	v2fout.suv.xy += (0.5 * v2fout.suv.w);
	v2fout.suv.x  += 0.5f * invWidth * v2fout.suv.w;
	v2fout.suv.y  += 0.5f * invHeight * v2fout.suv.w;
}

float4 fp(in v2f v2fin): COLOR0
{
	//Position buffer. 
	float4 pp     = float4(tex2Dproj(ps, v2fin.suv).xyz, 1);
	//Attenuation.
	float at      = distance(pp, lp);
	//Normal buffer.
	float3 np     = tex2Dproj(ns, v2fin.suv);
	//Colour buffer.
	float3 cp     = tex2Dproj(cs, v2fin.suv);
	//Calc light vector.
	float3 lv     = (lp - pp);
	float3 ld     = normalize(lv);
	//Calc lighting and attenuation.
	float  d      = saturate(dot(ld, np) * 0.5f + 0.5f) * clamp(1 - length(at) / lr, 0, 1);
	//Resulting pixel.
	return d * (float4(cp, 1) * float4(lc, 1));
}

technique render
{
	pass pass0
	{
		vertexshader     = compile vs_1_1 vp();
		pixelshader      = compile ps_2_0 fp();
		AlphaBlendEnable = True;
		SrcBlend         = One;
		DestBlend        = One;
//		CullMode         = CCW;
//		ZEnable          = False;
	}
}