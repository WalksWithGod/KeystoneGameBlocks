//Minimesh Light for ShadowSong.
//------------------------
//	Do not distribute.
//------------------------
//Written By Geoff Wilson.
//18/11/09

float4x3 w[52]: MINIMESH_WORLD;
float4x4 vip  : VIEWPROJECTION;
float4   c[52]: MINIMESH_COLOR;
float3   v    : VIEWPOS;

float iw, ih;

texture colour;
texture position;
texture normals;
texture view;

sampler2D cs  = sampler_state
{
	Texture   = (colour);
	MIPFILTER = LINEAR;
	MAGFILTER = LINEAR;
	MINFILTER = LINEAR;
	AddressU  = CLAMP;
	AddressV  = CLAMP;
};

sampler2D ps  = sampler_state
{
	Texture   = (position);
	MIPFILTER = LINEAR;
	MAGFILTER = LINEAR;
	MINFILTER = LINEAR;
	AddressU  = CLAMP;
	AddressV  = CLAMP;
};

sampler2D ns  = sampler_state
{
	Texture   = (normals);
	MIPFILTER = LINEAR;
	MAGFILTER = LINEAR;
	MINFILTER = LINEAR;
	AddressU  = CLAMP;
	AddressV  = CLAMP;
};

sampler2D vs  = sampler_state
{
	Texture   = (view);
	MIPFILTER = LINEAR;
	MAGFILTER = LINEAR;
	MINFILTER = LINEAR;
	AddressU  = CLAMP;
	AddressV  = CLAMP;
};

struct a2v
{
	float4 p  : POSITION0;
	float2 uv : TEXCOORD0;
	float  i  : TEXCOORD3; //Minimesh Light item index. 
};

struct v2f
{
	float4 p  : POSITION0;
	float2 uv : TEXCOORD0;
	float4 suv: TEXCOORD1;
	float3 lp : TEXCOORD2;
	float  lr : TEXCOORD3;
	float4 lc : TEXCOORD4;
};

//Simple specular function.
float calculatelyon(float3 vv, float3 lv, float3 nv)
{
	float3 hw = normalize(vv + lv);
	float3 d  = hw - nv;
	float  ss = saturate(dot(d, d) * 1.5f);
		   return pow(1 - ss, 3);
}

v2f vertex(in a2v a2vin)
{
	v2f v2fout;
	
	float4x3 wm             = w[a2vin.i];                            //Grab the world matrix for this minimesh item.
	float3   wp             = mul(a2vin.p, wm);                      //Position in world space.
			 v2fout.p       = mul(float4(wp, 1), vip);               //Move vertex into worldviewprojection/clip space.
			 v2fout.uv      = a2vin.uv;
			 v2fout.lc      = c[a2vin.i];                            //Grab the colour for this light.
			 v2fout.lr      = wm[2][2] - 0.5f;                       //Grab the light range from scale, make it slightly smaller.
			 v2fout.lp      = float3(wm[3][0], wm[3][1], wm[3][2]);  //Grab the light position.
			 
			 //Calc screen/projection co-ords.
			 v2fout.suv     = v2fout.p;
			 v2fout.suv.xy  = float2(v2fout.suv.x * 0.5f, -v2fout.suv.y * 0.5f);
			 float halfsuv_w = 0.5f * v2fout.suv.w; // Hypnotron - Nov.1.2011 added caching of this mult
			 v2fout.suv.xy += halfsuv_w; //(0.5f * v2fout.suv.w);
			 v2fout.suv.x  += halfsuv_w * iw; //0.5f * iw * v2fout.suv.w; 
			 v2fout.suv.y  += halfsuv_w * ih; //0.5f * ih * v2fout.suv.w;
	
			 return v2fout;
}

float4 fragment(in v2f v2fin): COLOR0
{
	float4 vp = tex2Dproj(vs, v2fin.suv);                                                     //View vector sample.
	float3 vv = normalize(vp.rgb);                                                            //Normalize view vector.
	float4 cp = tex2Dproj(cs, v2fin.suv);                                                     //Colour sample.
	float4 pp = float4(tex2Dproj(ps, v2fin.suv).xyz, 1);                                      //Position sample.
	float4 np = tex2Dproj(ns, v2fin.suv);                                                     //Normal sample.
	float3 lv = (v2fin.lp - pp);                                                              //Light vector.
	float3 ld = normalize(lv);                                                                //Normalize light vector.
	float  at = distance(pp, v2fin.lp);                                                       //Calc initial attenuation.
	float  d  = saturate(dot(np, ld) * 0.5f + 0.5f) * clamp(1 - length(at) / v2fin.lr, 0, 1); //Calc light * Final attenuation calc.
	float  s  = 0; //calculatelyon(vv, ld, np) * np.a;                                             //Calc specular * Spec map.
	
		   return d * ((cp * v2fin.lc) + s);                                                  //Output pixel.
}

technique render
{
	pass p
	{
		AlphaBlendEnable = True;
		SrcBlend         = One;
		DestBlend        = One;
		CullMode         = CW;
		ZEnable          = False;
		VertexShader     = compile vs_2_0 vertex();
		PixelShader      = compile ps_2_0 fragment();
	}
}

