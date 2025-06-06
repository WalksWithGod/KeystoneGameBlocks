// outer atmosphere radius
float AtmosphereRadius <
string UIName = "Atmosphere Radius";
string UIWidget = "Slider";
float UIMin = 0.0;
float UIMax = 10000.0;
float UIStep = 1.0; 
> = {1200.0f};


// planet surface radius
float SurfaceRadius <
string UIName = "Surface Radius";
string UIWidget = "Slider";
float UIMin = 0.0;
float UIMax = 10000.0;
float UIStep = 1.0; 
> = {1024.0f};

float4 gLamp0DirPos : LIGHTPOINT0_POSITION ; 

// this is the sun position/direction
//float4 gLamp0DirPos : POSITION < // or direction, if W==0
//string Object = "Light0";
//string UIName = "Lamp 0 Position/Direction";
//string Space = "LIGHT_COORDS";
//> = {10.0f,10.0f,10.0f,1.0};

// this is the atmosphere 2d gradient
texture gTex <
string ResourceName = "AtmosphereGradient";
string ResourceType = "2D";
string UIName = "Gradient Texture";
>;
sampler2D gTexSampler = sampler_state 
{
	Texture = gTex;
	Filter = MIN_MAG_MIP_LINEAR;
	AddressU = Clamp;
	AddressV = Clamp; 
};

// this is for setting where the horizon should fall on the sphere
float StretchAmt <
string UIName = "Stretch Amount";
string UIWidget = "Slider";
float UIMin = 0.0;
float UIMax = 1.0;
float UIStep = 0.01; 
> = {0.25f};

// this is for mie scattering
float Atmosphere_G <
string UIName = "Atmosphere G";
string UIWidget = "Slider";
float UIMin = -1.0;
float UIMax = -0.5;
float UIStep = 0.001; 
> = {-0.95f};

float4x4 WorldViewProj : WORLDVIEWPROJECTION;
float4x4 ViewIXf : VIEWINVERSE;
float4x4 WorldXf : WORLD;
float4x4 WorldIT : WORLDIT;


void main2VS
(
	float3 pos : POSITION,
	uniform float4 lightPos,

	out float4 oPosition: POSITION,
	out float2 oUV: TEXCOORD0,
	out float oAlpha: TEXCOORD1,
	out float3 oCamToPos: TEXCOORD2,
	out float3 oLightDir :TEXCOORD3
) 

{
	float4 Po = float4(pos.xyz,1);
	float4 Pw = mul(Po, WorldXf);
	float3 position = Po.xyz;
	float4 camPos = float4(ViewIXf[3].xyz,1);

	camPos = mul(camPos, transpose(WorldIT));
	lightPos = mul(lightPos, transpose(WorldIT));

	oPosition = mul(Po, WorldViewProj); 

	float radius = length(position);
	float radius2 = radius * radius; 
	float camHeight = length(camPos.xyz);
	float3 camToPos = position - camPos.xyz;
	float farDist = length(camToPos);

	float3 lightDir = normalize(lightPos.xyz);
	float3 normal = normalize(position);

	float3 rayDir = camToPos / farDist;
	float camHeight2 = camHeight * camHeight;

	// Calculate the closest intersection of the ray with the outer atmosphere
	float B = 2.0 * dot(camPos.xyz, rayDir);
	float C = camHeight2 - radius2;
	float det = max(0.0, B*B - 4.0 * C);
	float nearDist = 0.5 * (-B - sqrt(det));
	float3 nearPos = camPos.xyz + (rayDir * nearDist);
	float3 nearNormal = normalize(nearPos);

	// get dot products we need
	float lc = dot(lightDir, camPos / camHeight);
	float ln = dot(lightDir, normal);
	float lnn = dot(lightDir, nearNormal);

	// get distance to surface horizon
	float altitude = camHeight - SurfaceRadius;
	float horizonDist = sqrt((altitude*altitude) + (2.0 * SurfaceRadius * altitude));
	float maxDot = horizonDist / camHeight;

	// get distance to atmosphere horizon - use max(0,...) because we can go into the atmosphere
	altitude = max(0,camHeight - AtmosphereRadius);
	horizonDist = sqrt((altitude*altitude) + (2.0 * AtmosphereRadius * altitude));

	// without this, the shift between inside and outside atmosphere is jarring
	float tweakAmount = 0.1;
	float minDot = max(tweakAmount,horizonDist / camHeight);

	// scale minDot from 0 to -1 as we enter the atmosphere
	float minDot2 = ((camHeight - SurfaceRadius) * (1.0 / (AtmosphereRadius - SurfaceRadius))) - (1.0 - tweakAmount);
	minDot = min(minDot, minDot2);

	// get dot product of the vertex we're looking out
	float posDot = dot(camToPos / farDist,-camPos.xyz / camHeight) - minDot;

	// calculate the height from surface in range 0..1
	float height = posDot * (1.0 / (maxDot - minDot));

	// push the horizon back based on artistic taste
	ln = max(0,ln + StretchAmt);
	lnn = max(0,lnn + StretchAmt);

	// the front color is the sum of the near and far normals
	float brightness = saturate(ln + (lnn * lc));

	// use "saturate(lc + 1.0 + StretchAmt)" to make more of the sunset side color be used when behind the planet
	oUV.x = brightness * saturate(lc + 1.0 + StretchAmt);
	oUV.y = height;

	// as the camera gets lower in the atmosphere artificially increase the height
	// so that the alpha value gets raised and multiply the increase amount
	// by the dot product of the light and the vertex normal so that 
	// vertices closer to the sun are less transparent than vertices far from the sun.
	height -= min(0.0,minDot2 + (ln * minDot2));
	oAlpha = height * brightness;

	// normalised camera to position ray
	oCamToPos = -rayDir;

	oLightDir = normalize(lightPos.xyz - position.xyz); 
}

float4 mainBPS( float2 uv : TEXCOORD0,
	float alpha : TEXCOORD1,
	float3 camToPos : TEXCOORD2,
	float3 lightDir :TEXCOORD3,
	uniform sampler2D TexSampler
) : COLOR 
{

	const float fExposure = 1.5;
	float g = Atmosphere_G;
	float g2 = g * g;

	// atmosphere color
	float4 diffuse = tex2D(TexSampler,uv);

	// sun outer color - might could use atmosphere color
	float4 diffuse2 = tex2D(TexSampler,float2(min(0.5,uv.x),1));

	// this is equivilant but faster than fCos = dot(normalize(lightDir.xyz),normalize(camToPos));
	float fCos = dot(lightDir.xyz,camToPos) * rsqrt( dot(lightDir.xyz,lightDir.xyz) * dot(camToPos,camToPos));
	float fCos2 = fCos * fCos;

	// apply alpha to atmosphere
	float4 diffuseColor = diffuse * alpha;

	// sun glow color
	float fMiePhase = 1.5 * ((1.0 - g2) / (2.0 + g2)) * (1.0 + fCos2) /(1.0 + g2 - 2.0*g*fCos);
	float4 mieColor = diffuse2 * fMiePhase * alpha;

	// use exponential falloff because mie color is in high dynamic range
	// boost diffuse color near horizon because it gets desaturated by falloff
	return 1.0 - exp((diffuseColor * (1.0 + uv.y) + mieColor) * -fExposure);
}

technique technique1 
{
	pass p0 
	{
		//ZEnable = false;  
		ZWriteEnable = false;  
		AlphablendEnable = true;
		SrcBlend = One;
		DestBlend = InvSrcAlpha;
		AlphaTestEnable = false;
		VertexShader = compile vs_3_0 main2VS(gLamp0DirPos);
		PixelShader = compile ps_3_0 mainBPS(gTexSampler); 
	}
}