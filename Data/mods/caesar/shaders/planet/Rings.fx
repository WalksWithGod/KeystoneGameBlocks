
// -------------------------------------------------------------
// Semantics
// -------------------------------------------------------------
float4x4 World : WORLD;	
float4x4 View : VIEW;
float4x4 Projection : PROJECTION;
// todo: object_to_clip is for screenspace for deferred normally... right?
float4x4 object_to_clip : WORLDVIEWPROJECTION;
// todo: what i should do instead is pass two parameters, lightDirection and light Distance and this way i can cap the max distance
//       even set the max light distance as a constant and then use that to compute the light Position we need in our calculations.
float3 lightPosition :LIGHTPOINT0_POSITION;
float3 CameraPosition : VIEWPOS; 


// -------------------------------------------------------------
// Parameters
// -------------------------------------------------------------
float PlanetRadiusSquared = 511.1106064E+13; //24.068016E+13;  // the ring.fx doesn't inherently know the radius of the planet that will be casting a shadow
// jupiter value = 5111106064000000;
// 511.1106064E+13; //
float shadowIntensity = 0.15; // 1.0 = no shadow fx.  0.0 = full black shadow pixel

// distance where added alpha starts to be used to make 
// rings invisible as you get close or inside them
float BeginCameraFade = 50000; 

#if defined(LOG_Z)
const float C = 1.0;  
const float far = 1000000000.0;  
const float offset = 1.0;
#endif

// -------------------------------------------------------------
// Textures and Samplers
// -------------------------------------------------------------
texture ringtex : TEXTURE0;
texture noisetex : TEXTURE1;

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
    float4 rawPos : POSITION0;
    float2 TexCoord : TEXCOORD0;
    
};

struct VertexShaderOutput
{
    float4 pos_clip : POSITION0;
	float2 TexCoord : TEXCOORD0;
	float3 World    : TEXCOORD1; 
	
};

// -------------------------------------------------------------
// Vertex Shader function
// -------------------------------------------------------------
VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
    output.pos_clip = mul(input.rawPos, object_to_clip); 
	#if defined(LOG_Z)
    //float4 worldPosition = mul(input.rawPos, World);
    //float4 viewPosition = mul(worldPosition, View);
	//output.pos_clip = log(C * viewPosition.z + 1) / log(C * far + 1) * viewPosition.w;	
	output.pos_clip = log(C * IN.rawPos.z + 1) / log(C * far + 1) * IN.rawPos.w;
    #endif
	
	output.TexCoord = input.TexCoord;
    output.World = mul(input.rawPos, World).xyz; //worldPosition.xyz;

    return output; // output gets interpolated when arriving in the PixelShader
}

// -------------------------------------------------------------
// Pixel Shader function
// -------------------------------------------------------------
float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float4 noise=float4(0,0,0,0);
	
	// texture the pixel
	float2 tc = input.TexCoord;
	tc = (tc - 0.5) * 0.5;
	tc.x = length(tc) * 4;
	tc.y = 0;
    float4 result = tex2D(RingSampler,tc);
	// this hack computes alpha based on the rgb values of the lookup texture when that texture has no built in alpha set
	// so this should be a param in the shader where we either use the texture alpha or we use the grayscale
	// for color textures we should always enforce that .a be set in the png/dds/tga 
	result.a = (1.0 - result.r) / 1.0; // i think it's computing alpha from R value color.   
	// I dont know why it's not rgb though and just .r
	
	float3 distanceToCamera = CameraPosition - input.World;
	float blend = length(distanceToCamera);

	if (blend < BeginCameraFade)
	{
		// the amount of noise blending to do.  0.0 is pure noise.  1.0 = pure diffuse.
		blend /= BeginCameraFade; //blend = .7; 
		 
		noise=(tex2D(NoiseSampler, input.TexCoord));

		// increase the alpha (by making the value smaller) the closer the camera is to this pixel
		result.a *= blend;
	}
	else
	{
		blend=1;
	}
		
	
	// test if the planet sphere intersects the light ray and thus might cast a shadow
	float3 center = float3(World[3][0],World[3][1],World[3][2]);
	float3 lightDir = float3(0, 0, -1); // normalize(lightPosition - center);   // direction from the planet's center to the light source
	
	// todo: doesn't the planetradiussquared have to change as we scale the planet forward using scalefactor?
	lightPosition =  float3 (0,0, -PlanetRadiusSquared - 1000000); //center + (lightDir * PlanetRadiusSquared * 3); 
	lightDir = -(center - lightPosition);
	
	// test if the planet sphere intersects the light ray and thus might cast a shadow
//	float3 center = float3(World[3][0],World[3][1],World[3][2]);
//	float3 lightDir = center - lightPosition;   // direction from the planet's center to the light source
	
	// http://www.riemers.net/eng/Tutorials/XNA/Csharp/Series3/Per-pixel_colors.php  <-- VS sends to the interpolator
	// http://johnwhigham.blogspot.com/2011/11/planetary-rings.html
	// TODO: Is the lightposition for this ring in camera space?
	float3 ray = lightPosition - input.World ; // direction from light to position on the mesh in world coords
	float lr = length(ray);
	float ld = length(lightDir);  // 
	ray=normalize(ray);
	
	float B = dot(lightDir, ray);
	float C = dot(lightDir, lightDir) - PlanetRadiusSquared;
	float D = B * B - C;  
	
	//clip(IntersectRaySphere(PlanetRadiusSquared, m_v3ViewPosition, v3RayUnitVector) - fViewToPixelDistance);
	float shadow_i = 1.0; // disable shadow until we know if we this pixel is shadowed ;  
	if (D > 0)
	{
		// light does intersect the sphere
		if (lr > ld)
		{
			// only cast shadow on the back side of the sphere, not the front
			//result.xyz = 0;
			shadow_i = shadowIntensity; // use the global shadow intensity modifier
		}
	}
	
	result.xyz = lerp(noise.xyz, result.xyz, blend) * shadow_i;
	
	//input.Position = log(C * input.Position.z + 1) / log(C * far + 1) * input.Position.w;
    return result;
}

float IntersectRaySphere (
		const float fSphereRadiusSquared, // Squared radius of the sphere to intersect against
		const float3 v3RayOrigin, // Position of the ray origin
		const float3 v3RayUnitDirection) // Unit length vector representing the ray's direction away from the origin
{
	const float fB = dot(v3RayOrigin, v3RayUnitDirection);
	const float fD = sqrt((fB*fB) - (dot(v3RayOrigin, v3RayOrigin)-fSphereRadiusSquared));
	return -fB-fD;
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
