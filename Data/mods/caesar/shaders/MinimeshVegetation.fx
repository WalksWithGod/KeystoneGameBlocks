//Point-lit wind affected instances.

float Radius;                     //Light radius.
float windspeed             = 3;
float windpower             = 0.06;
float time                  : TIME;
float3 LightPos             : LIGHTPOINT0_POSITION;
float4x3 minimeshes[52]     : MINIMESH_WORLD;
float4 minimeshes_color[52] : MINIMESH_COLOR;
float4 minimeshes_fade      : MINIMESH_FADE;
float3 CameraPos            : CAMERAPOSITION;
float3 CameraDir            : CAMERADIRECTION; 
float4 LightCol;
float4x4 world              : WORLD;
float4x4 viewProj           : VIEWPROJECTION;
texture tex                 : TEXTURE0;
sampler samp1               : register(s0);

struct VS_STANDARD_VERTEX
{
    float4 position: POSITION;
    float3 normal  : NORMAL;
    float2 tex     : TEXCOORD0;
    float2 index   : TEXCOORD3;  // Minimesh Element Index is ALWAYS on TEXCOORD3.
    float3 tangent : TANGENT;
    float3 binormal : BINORMAL;
};

struct VS_OUTPUT
{
    float4 position: POSITION;
    float2 tex     : TEXCOORD0;
    float4 color   : COLOR0;
    float2 tex1    : TEXCOORD1;
};

VS_OUTPUT VS_MiniMesh(VS_STANDARD_VERTEX vertex)
{
	VS_OUTPUT o;
	float4x3 ourMatrix   = minimeshes[vertex.index.x];
	
	//Make wind.
	float3 floor         = (vertex.position.x, 30, vertex.position.z);
	vertex.position.z    += (sin(windspeed * time) * windpower) * distance(floor.y, vertex.position.y);
	vertex.position.x    += (sin(windspeed * time) * windpower) * distance(floor.y, vertex.position.y);
	//End wind.
	float3 worldPos      = mul(vertex.position, ourMatrix);
    float3 normal        = normalize(mul(vertex.normal, (float3x3)world).xyz);
    float3 light         = normalize(LightPos - worldPos); 
    float3 lightColor    = LightCol;
    float atten          = clamp(1 - length(LightPos.xyz - worldPos) / Radius, 0, 1);
    float lightIntensity = clamp(dot(normal, light), 0, 1) * atten;
    float4 color         = float4(lightColor * lightIntensity, 1.0f); 
    color.rgb            += 0.15f; 
	o.position           = mul(float4(worldPos, 1.0f), viewProj);
	o.tex                = vertex.tex;
	o.tex1               = vertex.tex; 
	o.color.a            = 1;
	o.color              = minimeshes_color[vertex.index.x] * clamp(color, 0, 1);

	return o;
} 

float4 PS_MiniMesh(VS_OUTPUT input) : COLOR0
{
	return input.color * tex2D(samp1,input.tex);
}


technique minimesh
{
	pass pass0
	{
		VertexShader = compile vs_2_0 VS_MiniMesh();
		PixelShader = compile ps_2_0 PS_MiniMesh();
	}
} 
