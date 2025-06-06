//Minimesh Light for ShadowSong.
//------------------------
//	Do not distribute.
//------------------------
//Written By Geoff Wilson.
//18/11/09

float4x3 world_matrix[52]: MINIMESH_WORLD;
float4x4 viewprojection  : VIEWPROJECTION;
float4   light_color[52]: MINIMESH_COLOR;

float viewport_width, viewport_height;

texture diffuse_tex;
texture position;
texture normals_tex;
texture view;

sampler2D diffuseSampler  = sampler_state
{
	Texture   = (diffuse_tex);
	MIPFILTER = LINEAR;
	MAGFILTER = LINEAR;
	MINFILTER = LINEAR;
	AddressU  = CLAMP;
	AddressV  = CLAMP;
};

sampler2D normalSampler  = sampler_state
{
	Texture   = (normals_tex);
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

sampler2D vs  = sampler_state
{
	Texture   = (view);
	MIPFILTER = LINEAR;
	MAGFILTER = LINEAR;
	MINFILTER = LINEAR;
	AddressU  = CLAMP;
	AddressV  = CLAMP;
};

struct vs_in
{
	float4 light_pos  : POSITION0;
	float2 uv : TEXCOORD0;
	float  light_index  : TEXCOORD3; //Minimesh Light item index. 
};

struct ps_in
{
	float4 p  : POSITION0;
	float2 uv : TEXCOORD0;
	float4 suv: TEXCOORD1;
	float3 light_position : TEXCOORD2;
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

ps_in vertex(in vs_in input)
{
	ps_in output;
	
	float4x3 wm             = world_matrix[input.light_index];                            //Grab the world matrix for this minimesh item.
	float3   wp             = mul(input.light_pos, wm);                      //Position in world space.
			 output.p       = mul(float4(wp, 1), viewprojection);               //Move vertex into worldviewprojection/clip space.
			 output.uv      = input.uv;
			 output.lc      = light_color[input.light_index];                            //Grab the colour for this light.
			 output.lr      = wm[2][2] - 0.5f;                       //Grab the light range from scale, make it slightly smaller.
			 output.light_position      = float3(wm[3][0], wm[3][1], wm[3][2]);  //Grab the light position.
			 
			 //Calc screen/projection co-ords.
			 output.suv     = output.p;
			 output.suv.xy  = float2(output.suv.x * 0.5f, -output.suv.y * 0.5f);
			 float halfsuv_w = 0.5f * output.suv.w; // Hypnotron - Nov.1.2011 added caching of this mult
			 output.suv.xy += halfsuv_w; 
			 output.suv.x  += halfsuv_w * viewport_width; 
			 output.suv.y  += halfsuv_w * viewport_height; 
	
			 return output;
}

float4 fragment(in ps_in input): COLOR0
{
	float4 vp = tex2Dproj(vs, input.suv);                                                     //View vector sample.	
	float3 vv = normalize(vp.rgb);                                                            //Normalize view vector.
	float4 diffuse = tex2Dproj(diffuseSampler, input.suv);                                                 
	float4 pp = float4(tex2Dproj(ps, input.suv).xyz, 1);                                      //Position sample.
	float4 np = tex2Dproj(normalSampler, input.suv); 
	float3 lightdir = normalize(input.light_position - pp);                                                            
	float  dist = distance(pp, input.light_position);    
	///Calc initial attenuation.
	// note on following line there is an extra ")" after 1 - dist that shouldn't be there.
	// result== CRASH
	float  d  = saturate(dot(np, lightdir) * 0.5f + 0.5f) * clamp(1 - dist / input.lr, 0, 1); //Calc light * Final attenuation calc.
	float  specular  = 0; //calculatelyon(vv, lightdir, np) * np.a;                                             //Calc specular * Spec map.
	
	return d * ((diffuse * input.lc) + specular);                                                  //Output pixel.
}

technique primary
{
	pass pass0
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

