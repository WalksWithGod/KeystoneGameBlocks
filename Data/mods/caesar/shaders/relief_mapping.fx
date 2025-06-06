// $Id: //sw/devrel/SDK/MEDIA/HLSL/relief_mapping.fx#1 $

string description = "Relief Mapping by Fabio Policarpo";

// Tweakable controls //////////////////////

float tile
<
	string UIName = "Tile Factor";
	string UIWidget = "slider";
	float UIMin = 1.0;
	float UIStep = 1.0;
	float UIMax = 32.0;
> = 8;

float depth_fact
<
	string UIName = "Depth Factor";
	string UIWidget = "slider";
	float UIMin = 0.0f;
	float UIStep = 0.05f;
	float UIMax = 1.0f;
> = 0.1;

float3 diffuse
<
	string UIName = "Diffuse";
	string UIWidget = "color";
> = {1,1,1};

float3 specular
<
	string UIName = "Specular";
	string UIWidget = "color";
> = {0.75,0.75,0.75};

float shine
<
    string UIName = "Shine";
	string UIWidget = "slider";
	float UIMin = 8.0f;
	float UIStep = 8;
	float UIMax = 256.0f;
> = 128.0;

float3 lightpos : POSITION
<
	string UIName="Light Position";
	string Object = "PointLight";
	string Space = "World";
> = { -300.0, 300.0, -300.0 };

// "Untweables" ///////////////////////

float4x4 modelviewproj : WorldViewProjection <string UIWidget="None";>;
float4x4 modelview : WorldView <string UIWidget="None";>;
float4x4 modelinv : WorldInverse <string UIWidget="None";>;
float4x4 view : View <string UIWidget="None";>;

// Textures /////////////////////////

texture texmap : DIFFUSE
<
    string ResourceName = "relief_rockwall.jpg";
    string ResourceType = "2D";
>;

texture reliefmap : NORMAL
<
    string ResourceName = "relief_rockwall.tga";
    string ResourceType = "2D";
>;

sampler2D texmap_sampler = sampler_state
{
	Texture = <texmap>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
};

sampler2D reliefmap_sampler = sampler_state
{
	Texture = <reliefmap>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
};

/////////////////////////////////

struct a2v 
{
    float4 pos		: POSITION;
    float4 color	: COLOR0;
    float3 normal	: NORMAL;
    float2 txcoord	: TEXCOORD0;
    float3 tangent	: TANGENT0;
    float3 binormal	: BINORMAL0;
};

struct v2f
{
    float4 hpos		: POSITION;
	float4 color	: COLOR0;
    float2 txcoord	: TEXCOORD0;
    float3 vpos		: TEXCOORD1;
    float3 tangent	: TEXCOORD2;
    float3 binormal	: TEXCOORD3;
    float3 normal	: TEXCOORD4;
	float4 lightpos	: TEXCOORD5;
};

// shaders ////////////////////////////

v2f view_space(a2v IN)
{
	v2f OUT;

	// vertex position in object space
	float4 pos=float4(IN.pos.x,IN.pos.y,IN.pos.z,1.0);

	// compute modelview rotation only part
	float3x3 modelviewrot;
	modelviewrot[0]=modelview[0].xyz;
	modelviewrot[1]=modelview[1].xyz;
	modelviewrot[2]=modelview[2].xyz;

	// vertex position in clip space
	OUT.hpos=mul(pos,modelviewproj);

	// vertex position in view space (with model transformations)
	OUT.vpos=mul(pos,modelview).xyz;

	// light position in view space
	float4 lp=float4(lightpos.x,lightpos.y,lightpos.z,1);
	OUT.lightpos=mul(lp,view);

	// tangent space vectors in view space (with model transformations)
	OUT.tangent=mul(IN.tangent,modelviewrot);
	OUT.binormal=mul(IN.binormal,modelviewrot);
	OUT.normal=mul(IN.normal,modelviewrot);
	
	// copy color and texture coordinates
	OUT.color=IN.color;
	OUT.txcoord=IN.txcoord.xy;

	return OUT;
}

float4 normal_map(
	v2f IN,
	uniform sampler2D texmap,
	uniform sampler2D reliefmap) : COLOR
{
	float4 normal=tex2D(reliefmap,IN.txcoord*tile);
	normal.xy=normal.xy*2.0-1.0; // trafsform to [-1,1] range
	normal.z=sqrt(1.0-dot(normal.xy,normal.xy)); // compute z component
	
	// transform normal to world space
	normal.xyz=normalize(normal.x*IN.tangent-normal.y*IN.binormal+normal.z*IN.normal);
	
	// color map
	float4 color=tex2D(texmap,IN.txcoord*tile);

	// view and light directions
	float3 v = normalize(IN.vpos);
	float3 l = normalize(IN.lightpos.xyz-IN.vpos);

	// compute diffuse and specular terms
	float att=saturate(dot(l,IN.normal.xyz));
	float diff=saturate(dot(l,normal.xyz));
	float spec=saturate(dot(normalize(l-v),normal.xyz));

	// compute final color
	float4 finalcolor;
	finalcolor.xyz=att*(color.xyz*diffuse.xyz*diff+specular.xyz*pow(spec,shine));
	finalcolor.w=1.0;

	return finalcolor;
}

float4 parallax_map(
	v2f IN,
	uniform sampler2D texmap,
	uniform sampler2D reliefmap) : COLOR
{
   	// view and light directions
	float3 v = normalize(IN.vpos);
	float3 l = normalize(IN.lightpos.xyz-IN.vpos);

	float2 uv = IN.txcoord*tile;

	// parallax code
	float3x3 tbn = float3x3(IN.tangent,IN.binormal,IN.normal);
	float height = tex2D(reliefmap,uv).w * 0.06 - 0.03;
	uv += height * mul(tbn,v);

	// normal map
	float4 normal=tex2D(reliefmap,uv);
	normal.xy=normal.xy*2.0-1.0; // trafsform to [-1,1] range
	normal.z=sqrt(1.0-dot(normal.xy,normal.xy)); // compute z component

	// transform normal to world space
	normal.xyz=normalize(normal.x*IN.tangent-normal.y*IN.binormal+normal.z*IN.normal);

	// color map
	float4 color=tex2D(texmap,uv);

	// compute diffuse and specular terms
	float att=saturate(dot(l,IN.normal.xyz));
	float diff=saturate(dot(l,normal.xyz));
	float spec=saturate(dot(normalize(l-v),normal.xyz));

	// compute final color
	float4 finalcolor;
	finalcolor.xyz=att*(color.xyz*diffuse.xyz*diff+specular.xyz*pow(spec,shine));
	finalcolor.w=1.0;

	return finalcolor;
}

float ray_intersect_rm(
		in sampler2D reliefmap,
		in float2 dp, 
		in float2 ds)
{
	const int linear_search_steps=16;
	const int binary_search_steps=6;
	float depth_step=1.0/linear_search_steps;

	// current size of search window
	float size=depth_step;
	// current depth position
	float depth=0.0;
	// best match found (starts with last position 1.0)
	float best_depth=1.0;

	// search front to back for first point inside object
	for( int i=0;i<linear_search_steps-1;i++ )
	{
		depth+=size;
		float4 t=tex2D(reliefmap,dp+ds*depth);

		if (best_depth>0.996)	// if no depth found yet
		if (depth>=t.w)
			best_depth=depth;	// store best depth
	}
	depth=best_depth;
	
	// recurse around first point (depth) for closest match
	for( int i=0;i<binary_search_steps;i++ )
	{
		size*=0.5;
		float4 t=tex2D(reliefmap,dp+ds*depth);
		if (depth>=t.w)
		{
			best_depth=depth;
			depth-=2*size;
		}
		depth+=size;
	}

	return best_depth;
}

float4 relief_map(
	v2f IN,
	uniform sampler2D texmap,
	uniform sampler2D reliefmap) : COLOR
{
	float4 t;
	float3 p,v,l,s,c;
	float2 dp,ds,uv;
	float d;

	// ray intersect in view direction
	p  = IN.vpos;
	v  = normalize(p);
	s  = normalize(float3(dot(v,IN.tangent),dot(v,IN.binormal),dot(v,IN.normal)));
	s  *= depth_fact*0.2/dot(IN.normal,-v);
	dp = IN.txcoord*tile;
	ds = s.xy;
	d  = ray_intersect_rm(reliefmap,dp,ds);
	
	// get rm and color texture points
	uv=dp+ds*d;
	t=tex2D(reliefmap,uv);
	c=tex2D(texmap,uv);

	// expand normal from normal map in local polygon space
	t.xy=t.xy*2.0-1.0;
	t.z=sqrt(1.0-dot(t.xy,t.xy));
	t.xyz=normalize(t.x*IN.tangent-t.y*IN.binormal+t.z*IN.normal);

	// compute light direction
	p += s*d;
	l=normalize(p-IN.lightpos.xyz);
	
	// compute diffuse and specular terms
	float att=saturate(dot(-l,IN.normal.xyz));
	float diff=saturate(dot(-l,t.xyz));
	float spec=saturate(dot(normalize(-l-v),t.xyz));

	// compute final color
	float4 finalcolor;
	finalcolor.xyz=att*(c*diffuse*diff+specular.xyz*pow(spec,shine));
	finalcolor.w=1.0;

	return finalcolor;
}

float4 relief_map_shadows(
	v2f IN,
	uniform sampler2D texmap,
	uniform sampler2D reliefmap) : COLOR
{
	float4 t;
	float3 p,v,l,s,c;
	float2 dp,ds,uv;
	float d;

	// ray intersect in view direction
	p  = IN.vpos;
	v  = normalize(p);
	s  = normalize(float3(dot(v,IN.tangent),dot(v,IN.binormal),dot(v,IN.normal)));
	s  *= depth_fact*0.2/dot(IN.normal,-v);
	dp = IN.txcoord*tile;
	ds = s.xy;
	d  = ray_intersect_rm(reliefmap,dp,ds);
	
	// get rm and color texture points
	uv=dp+ds*d;
	t=tex2D(reliefmap,uv);
	c=tex2D(texmap,uv);

	// expand normal from normal map in local polygon space
	t.xy=t.xy*2.0-1.0;
	t.z=sqrt(1.0-dot(t.xy,t.xy));
	t.xyz=normalize(t.x*IN.tangent-t.y*IN.binormal+t.z*IN.normal);

	// compute light direction
	p += s*d;
	l=normalize(p-IN.lightpos.xyz);

	// ray intersect in light direction
	dp+= ds*d;
	s  = normalize(float3(dot(l,IN.tangent),dot(l,IN.binormal),dot(l,IN.normal)));
	s *= depth_fact*0.2/dot(IN.normal,-l);
	dp-= d*s.xy;
	ds = s.xy;
	float dl = ray_intersect_rm(reliefmap,dp,s.xy);
	if (dl<d-0.05) // if pixel in shadow
	{
		c*=0.4;
		specular=0;
	}

	// compute diffuse and specular terms
	float att=saturate(dot(-l,IN.normal.xyz));
	float diff=saturate(dot(-l,t.xyz));
	float spec=saturate(dot(normalize(-l-v),t.xyz));

	// compute final color
	float4 finalcolor;
	finalcolor.xyz=att*(c*diffuse*diff+specular.xyz*pow(spec,shine));
	finalcolor.w=1.0;

	return finalcolor;
}

// techniques /////////////////////////////////////////

technique normal_mapping
{
    pass p0 
    {		
		VertexShader = compile vs_1_1 view_space();
		PixelShader  = compile ps_2_0 normal_map(texmap_sampler,reliefmap_sampler);
    }
}

technique parallax_mapping
{
    pass p0 
    {		
		VertexShader = compile vs_1_1 view_space();
		PixelShader  = compile ps_2_0 parallax_map(texmap_sampler,reliefmap_sampler);
    }
}

technique relief_mapping
{
    pass p0 
    {		
		VertexShader = compile vs_1_1 view_space();
		PixelShader  = compile ps_2_a relief_map(texmap_sampler,reliefmap_sampler);
    }
}

technique relief_mapping_shadows
{
    pass p0 
    {		
		VertexShader = compile vs_1_1 view_space();
		PixelShader  = compile ps_2_a relief_map_shadows(texmap_sampler,reliefmap_sampler);
    }
}
