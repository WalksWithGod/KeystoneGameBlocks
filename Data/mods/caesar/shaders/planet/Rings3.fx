
// -------------------------------------------------------------
// Semantics
// -------------------------------------------------------------
float4x4 World 			: WORLD;	
float4x4 View 			: VIEW;
float4x4 Projection 	: PROJECTION;
// todo: what i should do instead is pass two parameters, lightDirection and light Distance and this way i can cap the max distance
//       even set the max light distance as a constant and then use that to compute the light Position we need in our calculations.
float3 LightPosition; // 	: LIGHTPOINT0_POSITION; 3.9.2017 - assigning via world domain object
float3 LightDirection 	: LIGHTDIR0_DIRECTION;
float3 CameraPosition 	: VIEWPOS; //CAMERAPOSITION;


// -------------------------------------------------------------
// Parameters
// -------------------------------------------------------------
float PlanetRadiusSquared = 5101102; // value assigned in domain_object_world.css 
float LIGHT_DISTANCE = 100000;

// distance where added alpha starts to be used to make 
// rings invisible as you get close or inside them
float BeginCameraFade = 5000000; 

#if defined(LOG_Z)
const float C = 1.0;  
const float far = 1000000000.0;  
const float offset = 1.0;
#endif

// -------------------------------------------------------------
// Textures and Samplers
// -------------------------------------------------------------
texture ringtex : TEXTURE0;
sampler RingSampler = sampler_state 
{
    	texture = <ringtex>;
	MagFilter = Anisotropic;
	MinFilter = Anisotropic;
	MipFilter = Anisotropic;
	//magfilter	= LINEAR; 
	//minfilter	= LINEAR; 
	//mipfilter	= LINEAR; 
	AddressU	= CLAMP;  
	AddressV	= CLAMP;
};

texture noisetex : TEXTURE1;
sampler NoiseSampler = sampler_state 
{
    	texture = <noisetex>;
	MagFilter = Anisotropic;
	MinFilter = Anisotropic;
	MipFilter = Anisotropic;
    	//magfilter	= LINEAR; 
    	//minfilter	= LINEAR; 
    	//mipfilter	= LINEAR; 
	AddressU	= CLAMP;  
	AddressV	= CLAMP;
};

// -------------------------------------------------------------
// Input/Output channels
// -------------------------------------------------------------
struct VertexShaderInput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
    
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
	float3 World    : TEXCOORD1; 
	
};

// -------------------------------------------------------------
// Vertex Shader function
// -------------------------------------------------------------
VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection); 
	#if defined(LOG_Z)
		output.Position = log(C * viewPosition.z + 1) / log(C * far + 1) * viewPosition.w;	
    #endif
	
	output.TexCoord= input.TexCoord;
    output.World=  worldPosition.xyz;

    return output; // output gets interpolated when arriving in the PixelShader
}

// -------------------------------------------------------------
// Pixel Shader function
// -------------------------------------------------------------
float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float4 noise=float4(0,0,0,0);
	
	// texture the pixel - work out how far from the centre of the planet a pixel is, and then use that to lookup the colour.
	float2 tc = input.TexCoord;
	tc = (tc - 0.5) * 0.5;
	tc.x = length(tc) * 4;
	tc.y = 0;
    	float4 result = tex2D(RingSampler,tc);
	// this hack computes alpha based on the rgb values of the lookup texture when that texture has no built in alpha set
	// so this should be a param in the shader where we either use the texture alpha or we use the grayscale
	// for color textures we should always enforce that .w be set in the png/dds/tga 
	result.w = (1.0f - result.x) / 1.0f; 
	
	float3 distance = CameraPosition - input.World;
	float blend = length(distance);


	if (blend < BeginCameraFade)
	{
		// the amount of noise blending to do.  0.0 is pure noise.  1.0 = pure diffuse.
		blend /= BeginCameraFade; //blend = .7; 
		 
		noise=(tex2D(NoiseSampler,input.TexCoord));

		// increase the alpha the closer the camera is to this pixel
		result.w *= blend;
	}
	else
	{
		blend = 1.0f;
	}
	

	
	// test if the planet sphere intersects the light ray and thus might cast a shadow
	float3 lightDir = float3 (0, 0, 1);
	
	float3 planetPos = float3(World[3][0], World[3][1], World[3][2]);
	float3 LightPosition = planetPos + (-lightDir * LIGHT_DISTANCE);
	float3 dst = LightPosition; // - planetPos;  // direction from the planet's position to the light source
	
	// http://www.riemers.net/eng/Tutorials/XNA/Csharp/Series3/Per-pixel_colors.php  <-- VS sends to the interpolator
    float3 ray = LightPosition - input.World ; // direction from light to position on the mesh in world coords
    float lr = length(ray);
    float ld = length(dst);  // 
    ray = normalize(ray);
	
	float B = dot(dst, ray);
	float C = dot(dst, dst) - PlanetRadiusSquared;
	float D = B * B - C;
	
	if (D > 0)
	{
		// light does intersect the sphere
		if (lr < ld)
		{
			// only cast shadow on the back side of the sphere, not the front
			result.xyz = 0;
		}
	}
	
	result.xyz = lerp(noise.xyz, result.xyz, blend);
	
	//input.Position = log(C * input.Position.z + 1) / log(C * far + 1) * input.Position.w;
	
    return result;
}

// -------------------------------------------------------------
// Technique
// -------------------------------------------------------------
technique primary
{
    pass pass0
    {
		// note: its best to not include state changes in the technique since your app code should do that
		// and then you can reduce state changes. This seems to be the concensus of gamedev.net folks. 
		// All we need to do is set  mesh.BlendingMode = CONST_TV_BLENDINGMODE.TV_BLEND_ALPHA and it works 
		// as well as setting renderstates directly in the shader.
		// note: do not uncomment.  
		//AlphaBlendEnable=true;
		//SrcBlend=srcalpha;
		//DestBlend=invsrcColor;
	
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
