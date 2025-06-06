
// -------------------------------------------------------------
// Semantics
// -------------------------------------------------------------
float4x3 world_matrix[52]: MINIMESH_WORLD;
float4x4 viewprojection  : VIEWPROJECTION;
float4   light_color[52]: MINIMESH_COLOR;

// -------------------------------------------------------------
// Other Parameters
// -------------------------------------------------------------
float lightIntensity = 1.0f; // can be used to make a light dimmer to simulate electrical brownout

float viewport_width;
float viewport_height;

// -------------------------------------------------------------
// Textures & Samplers
// -------------------------------------------------------------
texture color_tex;
texture normals_tex;
texture position;
texture view;

sampler2D diffuseSampler  = sampler_state
{
	Texture   = (color_tex);
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

// -------------------------------------------------------------
// Input/Output channels
// -------------------------------------------------------------
struct vs_in
{
	float4 light_pos  : POSITION0;
	float2 uv : TEXCOORD0;
	float  light_index  : TEXCOORD3; //Minimesh Light item index. 
};

struct ps_in
{
	float4 position : POSITION0;
	float2 uv : TEXCOORD0;
	float4 screenspace_uv: TEXCOORD1;
	float3 light_position : TEXCOORD2;
	float  light_range : TEXCOORD3;
	float4 light_color : TEXCOORD4;
};

//Simple specular function.
float calculatelyon(float3 view_vec, float3 light_dir, float3 normal_vec)
{
	float3 hw = normalize(view_vec + light_dir);
	float3 d  = hw - normal_vec;
	float  ss = saturate(dot(d, d) * 1.5f);
		   return pow(1 - ss, 3);
		   
	////reflection vector
    //float3 reflectionVector = normalize(reflect(-lightVector, normal));
    ////camera-to-surface vector
    //float3 directionToCamera = normalize(cameraPosition - position);
    /////compute specular light
    //float specularLight = specularIntensity * pow( saturate(dot(reflectionVector, directionToCamera)), specularPower);

    ////take into account attenuation and lightIntensity.
    //return attenuation * lightIntensity * float4(diffuseLight.rgb,specularLight);
}

// -------------------------------------------------------------
// Vertex Shader functions
// -------------------------------------------------------------
ps_in vertex(in vs_in input)
{
	ps_in output;
	
	float4x3 worldmat             = world_matrix[input.light_index];                     
	float3   worldpos             = mul(input.light_pos, worldmat);     
			 // position of the minimesh sphere vertex
			 output.position       = mul(float4(worldpos, 1), viewprojection);          
			 output.uv      = input.uv;
			 output.light_color      = light_color[input.light_index];                    
			 output.light_range      = worldmat[2][2] - 0.5f;   
			 // position of just the light, not the current vertex
			 output.light_position      = float3(worldmat[3][0], worldmat[3][1], worldmat[3][2]);  
			 
			 output.screenspace_uv     = output.position;
			 output.screenspace_uv.xy  = float2(output.screenspace_uv.x * 0.5f, -output.screenspace_uv.y * 0.5f);
			 float halfsuv_w = 0.5f * output.screenspace_uv.w; 
			 output.screenspace_uv.xy += halfsuv_w; 
			 output.screenspace_uv.x  += halfsuv_w * viewport_width; 
			 output.screenspace_uv.y  += halfsuv_w * viewport_height; 
	
			 return output;
}

// http://unity3d.com/support/documentation/Components/SL-SurfaceShaderLightingExamples.html
// E:\dev\_projects\_TV\Zak_IsotropicLightingModels\IsotropicLightingModels\bin\x86\Release\Content\IsotropicLighting.fx
// -------------------------------------------------------------
// Pixel Shader functions
// -------------------------------------------------------------
float4 fragment(in ps_in input): COLOR0
{
	float4 pixel_diffuse = float4(tex2Dproj(diffuseSampler, input.screenspace_uv).rgb, 1);  

	float specular_intensity = pixel_diffuse.a;
	float3 pixel_normal = tex2Dproj(normalSampler, input.screenspace_uv).xyz;
	float3 pixel_pos = tex2Dproj(ps, input.screenspace_uv).xyz;
	float3 lightdir = input.light_position - pixel_pos;    
	float  dist = length(lightdir);      // distance for determining attenuation.
	lightdir = normalize(lightdir);
	
	// linear attenuation
	float attenuation = 1 - dist /input.light_range;
	
	// Lambert lighting equation
	// max to filter out negative values
    float NdotL = max(0,dot(pixel_normal,lightdir));
    
    // WrapLambert (wrapped diffuse) - a modification of Diffuse lighting, where illumination 
    // "wraps around" the edges of objects. It's useful for faking subsurface scattering
    // effect. Again, the surface shader itself did not change at all, we're just using 
    // different lighting function.

    half diff = NdotL * 0.5 + 0.5; // wrapped labmert

    // saturate function - Clamps the specified value within the range of 0 to 1.
    
    
   
    //float3 diffuseLight = NdotL * input.light_color;
    
    //return attenuation * lightIntensity * float4(diffuseLight.rgb,0);
    
    
//	float  diffuse_lighting; // = saturate(NdotL * 0.5f + 0.5f) * clamp(attenuation, 0, 1); //Calc light * Final attenuation calc.
//	diffuse_lighting = saturate(input.light_color * lightIntensity * NdL);
	
	//float3 view_vec = normalize(tex2Dproj(vs, input.screenspace_uv).rgb);                          
	float  specular_lighting  = 0; //calculatelyon(view_vec, lightdir, pixel_normal) * specular_intensity;                            //Calc specular * Spec map.
	
	//return pixel_diffuse * ((diffuse_lighting + specular_lighting);
//	return diffuse_lighting * ((pixel_diffuse *  input.light_color) + specular_lighting);   

	return pixel_diffuse * input.light_color * ( diff * attenuation * 2) + specular_lighting;                                    
}

technique primary  // primary for sm2 which cannot take advantage of built in depth buffer and must use render target
{
	pass pass0
	{
		CullMode         = CW;
		ZEnable          = False;
		VertexShader     = compile vs_2_0 vertex();
		PixelShader      = compile ps_2_0 fragment();
	}
}

