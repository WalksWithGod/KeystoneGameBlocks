// =============================================================
// Planet Normal-Mapping Shader
// 
// Copyright (c) 2006 Renaud B�dard (Zaknafein)
// http://zaknafein.no-ip.org (renaud.bedard@gmail.com)
// =============================================================
//
// -------------------------------------------------------------
// Compilation switches
// -------------------------------------------------------------
// #define FORWARD
// 		- only up to 4 directional lights supported
//      - pointlights or spotlights not supported.
//
// #define DEFERRED 
//
// #define DIFFUSEMAP_x2
// ----------------
// #define NORMALMAP
// 		#define PARALLAX_IN_NORMALMAP_ALPHA  // requires NORMALMAP 
// #define DIFFUSEMAP
// 		#define SPECULAR_IN_DIFFUSEMAP_ALPHA  // requires diffuse map
// #define SPECULARMAP 
// #define EMISSIVEMAP
// ----------------

// -------------------------------------------------------------
// Semantics
// -------------------------------------------------------------
float4x4 object_to_clip : WORLDVIEWPROJECTION;	
float4x4 matWorldIT : WORLDINVERSETRANSPOSE;
float4x4 world : WORLD;	
float4 view : VIEWPOS;
float3 lightPos : LIGHTPOINT0_POSITION;
float3 lightCol : LIGHTPOINT0_COLOR;
//float3 lightVec : LIGHTDIR0_DIRECTION; // = float3 (0,1,0);  LIGHTPOINT0_POSITION; // position of primary star
//float3 lightCol : LIGHTDIR0_COLOR;     // LIGHTPOINT0_COLOR; // range is treated as infinite
                                       // During visibility pass, we assign the lights to be used
									   // and these lights must be passed to the shader
									   // question now is, what is an elegant way to assign
									   // shader parameters?
									   // Well, we do want to use the scripts yes?
									   // Planet.DomainObject.Execute["render"];
									   // update()
									   // {
									   //      // get the 4 closest pointlights with the largets ranges restricted to the sector this entity is in and sort by range
									   //      string[] lightIDs = EntityAPI.GetAffectedPointLights(-1, 4, true, true));
									   //      if (lightIDs != null && lightIDs.Length > 0)
									   //          for (int i = 0; i < lightIDs.Length; i++)
									   //          {
									   //               // our starlight uses a direction as if it were a dir light
									   //               // and we do not attenuate.
									   //
									   //               // get the position and color of these lights
									   //
									   //				// assign the shader position and color of these
									   //               // lights 
									   //               EntityAPI.SetShaderParameter ("starlight0_direction");
									   
									   //               // shader max support 4 lights
									   //               if (i == 3) break;
									   //          }
									   // }
									   
									   // 1 - add a domain object to a planet 
									   // 2 - add a "render" method to it
									   // 		- add support for the "render" method to be registered by Script
									   // 3 - in the render method, randomize color types.Color.Randomize()
									   // 4 - make call to VisualFX.SetShaderParameterVector
									   // *** How do we prevent "render" script from calling when we want to 
									   //     do something like a depth pass and dont care about shaders
#if defined(LOG_Z)					   //     perhaps we add an arg (bool updateShaders)
const float C = 1.0;  
const float far = 1000000000.0;  
const float offset = 1.0;
#endif

#if defined (NORMALMAP)
// Specular power values for arithmetic path
float SPEC_POW_INTERVAL = 2.5f;
#endif

#if defined(PARALLAX_IN_NORMALMAP_ALPHA)
// x component = parallax scale
// y component = parallax bias
float2 scale_bias = float2(0.04f, 0.02f);
#endif

// -------------------------------------------------------------
// Textures and Samplers
// -------------------------------------------------------------
texture diffuseSpecMap : TEXTURE0; // specular in alpha channel
#if defined (DIFFUSEMAP_x2)
texture diffuseSpecMap2 : TEXTURE1; // western hemisphere
#endif

#if defined (NORMALMAP)
	#if defined (DIFFUSEMAP_x2)
	texture normalMap : TEXTURE2; // WARNING! Here it's looking for normalmap in TEXTURE SLOT #2.
	#else
	texture normalMap : TEXTURE1;  // can contain parallax height info in alpha channel
	#endif
#endif

#if defined (EMISSIVEMAP)
texture emissiveMap : TEXTURE3;  
#endif

sampler diffuseSampler = sampler_state 
{
	Texture = (diffuseSpecMap);
	MagFilter = Anisotropic;
	MinFilter = Anisotropic;
	MipFilter = Anisotropic;
	AddressU = Clamp;
	AddressV = Clamp;
	AddressW = Clamp;	
};

#if defined (NORMALMAP)  // required for specular and for parallax
sampler normalSampler = sampler_state 
{
	Texture = (normalMap);
	MagFilter = Anisotropic;
	MinFilter = Anisotropic;
	MipFilter = Anisotropic;
};
#endif 

#if defined(DIFFUSEMAP_x2)
sampler diffuseSampler2 = sampler_state // western hemisphere 
{
	Texture = (diffuseSpecMap2);
	MagFilter = Anisotropic;
	MinFilter = Anisotropic;
	MipFilter = Anisotropic;
	AddressU = Clamp;
	AddressV = Clamp;
	AddressW = Clamp;		
};
#endif

#if defined(EMISSIVEMAP)
sampler sampEmissive = sampler_state 
{
	Texture = (emissiveMap);
	MagFilter = Anisotropic;
	MinFilter = Anisotropic;
	MipFilter = Anisotropic;
};
#endif

// -------------------------------------------------------------
// Input/Output channels
// -------------------------------------------------------------
struct VS_INPUT 
{
	float4 rawPos : POSITION;		// Vertex position in object space
	float4 n : NORMAL;		// Vertex normal in object space
	float4 b : BINORMAL;	// Vertex binormal in object space
	float4 t : TANGENT;	// Vertex tangent in object space
	float2 uv : TEXCOORD0;	// Vertex texture coordinate
};

struct VS_OUTPUT 
{
	float4 pos_clip : POSITION;	// Transformed position
	float2 uv : TEXCOORD0;		// Interpolated & scaled t.c.
	float3 tangent_space_view : TEXCOORD1;			// Eye vector in tangent space
	float3 lightVec : TEXCOORD2;		// Light vector in tangent space
};
#define	PS_INPUT VS_OUTPUT		// What comes out of VS goes into PS!

#if defined(DEFERRED)
struct mrt_pixels 
{
	float4 Color: COLOR0; //Color buffer.
	float4 Position: COLOR1; //Position buffer.
	float4 NormalRGB_SpecA: COLOR2; //Normal(RGB) and Spec map(A) buffer.
	float4 ViewRGB_DepthA: COLOR3; //View vector(RGB) and depth(A) buffer.
};
#endif

// -------------------------------------------------------------
// General functions
// -------------------------------------------------------------
// -------------------------------------------------------------
// Parralax-Mapping pixel fragment
// -------------------------------------------------------------
#if defined(PARALLAX_IN_NORMALMAP_ALPHA)
float2 parallaxTexCoord(float2 oldCoord, float2 vecViewXY) 
{
	// our height map is stored in alpha channel of normal map
	// if you want to put it in a different texture, just specify 
	// the proper texture sampler and channel (i.e. r,g,b or a)
	float height = tex2D(normalSampler, oldCoord).a;
	height *= scale_bias.x - scale_bias.y;
	// offset the UV so below we select normals from normalSampler on those modified UVs
	oldCoord += (height * vecViewXY); 
	return oldCoord;
}
#endif

// -------------------------------------------------------------
// Vertex Shader function
// -------------------------------------------------------------
VS_OUTPUT VS(VS_INPUT IN) 
{
	VS_OUTPUT OUT;
    
    // Basic transformation into clip-space
    OUT.pos_clip = mul(IN.rawPos, object_to_clip);
	#if defined(LOG_Z)
    OUT.pos_clip = log(C * IN.rawPos.z + 1) / log(C * far + 1) * IN.rawPos.w;
	#endif
	
    float3x3 TBNMatrix = 
	{
		(float3)IN.t,
		(float3)IN.b,
		(float3)IN.n
	};	    
	
	float4x3 matWorldCast = (float4x3)world;
    
    // The TBN (Tangent-Binormal-Normal) Matrix transforms vectors
    // from world-space to tangent-space
    float3x4 worldITToTangentSpace = mul(TBNMatrix, matWorldIT);
    float3x3 worldToTangentSpace = mul(TBNMatrix, matWorldCast);

    // Since the light position is in world-space,...
    float4 worldPos = mul(IN.rawPos, world);

    // Get the view vector and the light vector in tangent-space
    OUT.tangent_space_view = mul(worldToTangentSpace, view - worldPos);
	float3 lightVec = normalize(lightPos - IN.rawPos.xyz);
    OUT.lightVec = mul(worldITToTangentSpace, lightVec);
	
    // Since the TextureMod commands do not affect the coordinates,
    // we need to supply and apply them ourselves
    OUT.uv = IN.uv;
    
	return OUT;
}

// -------------------------------------------------------------
// Pixel Shader function
// -------------------------------------------------------------

#if defined(DEFERRED)
mrt_pixels ps_deferred(in PS_INPUT input)
{
	mrt_pixels output;
	
	float3x3 tbn_world  = float3x3(input.t, input.b, input.n); 
	//If you want to clip pixels that are alpha 0.2f or lower in the diffuseMap alpha channel.
	//clip(tex2D(diffuseSampler, input.uv).a - 0.3f);
	float distance = length(input.pos_world.xyz - view.xyz);
    float2 newUV = input.uv;
	#if defined(PARALLAX_IN_NORMALMAP_ALPHA)
	float3 view_vec = normalize(input.tangent_space_view);
	// our height map is stored in alpha channel of normal map
	// if you want to put it in a different texture, just specify 
	// the proper texture sampler and channel (i.e. r,g,b or a)
	float height = tex2D(bumpSampler, input.uv).a;
	height *= scale_bias.x - scale_bias.y;
	// offset the UV so below we select normals from bumpSampler on those modified UVs
	newUV += (height * view_vec.xy); 
	#endif
	
	float4   ns   = tex2D(bumpSampler, newUV);  
	// image rgb are always 0 - 1 range.  
	// So we have to transform the rgb channels to [-1,1] because the range for normals is -1 to 1.
	// but not the alpha channel since it doesnt contain normal data.
	float3   n    = 2.0f * ns.rgb - 1.0f;       
	#if defined(PARALLAX_IN_NORMALMAP_ALPHA)
	float3   vp   = input.tangent_space_view; 
	#else
	// can we just use per vertex calc'd version and not per pixel? even below? eg. permanently move to vs for non parallax versiion             
	float3   vp   = mul(tbn_world, (view - input.pos_world));                             
	#endif
	 // Buffer outputs must match the render targets bit depth
	 // Also all render targets must be same bit depth.
	 // NOTE:  does not include a specular intensity in the .Color.A
     // instead he locks it at 1.0	 
	 // One work around could be in setting a specular_power Param that
	 // can be set
	 output.Color = tex2D(diffuseSampler, newUV);
	 output.Position = input.pos_world;
	 output.NormalRGB_SpecA = float4(mul(n, tbn_world), ns.a);
	 output.ViewRGB_DepthA = float4(vp, distance);
	 
	 return output;
}
#else
float4 ps_forward(PS_INPUT IN) : COLOR 
{
	float3 viewVec, lightVec;
	float2 texCoord;
	
	// If normalization is in the pixel shader (better precision)	
	viewVec = normalize(IN.tangent_space_view);
	lightVec = normalize(IN.lightVec);
	
	// Parallax mapping, if enabled
	#if defined(PARALLAX_IN_NORMALMAP_ALPHA)
		texCoord = parallaxTexCoord(IN.uv, viewVec.xy);
	#else
		texCoord = IN.uv;
	#endif
	
	#if defined(NORMALMAP)
	float3 sampledNormal = tex2D(normalSampler, texCoord).rgb;
	// Normal for diffuse lightning is the uncompressed sampled one
	float3 diffNormal = 2.0f * sampledNormal - 1.0f;
	// Specular normal can be attenuated
	float3 specNormal = diffNormal;
	
	// Specular reflection vector for Phong
	// MSDN	->		L - 2 * (N dot L) * N
	// W.Engel ->	2 * (N dot L) * N - L
	// Using W.Engel...
	float3 reflectVec = 2 * dot(specNormal, lightVec) * specNormal - lightVec;
	reflectVec = normalize(reflectVec);
	#endif
	
	float4 colDiffMap;
	float colGloss;
	float2 corrTC; // corrected texture coordinate taking into account potentiall 2 diffuse maps
	// Maps sampling
	// Hypnotron - I commented out this block so we dont have to use the two seperate maps
	//             But actually i think Zak is right that some cards wont support texture sizes
	//             greater than 2048x2048.  Well, I know my FX5200 did and that's sort of low endish
	//		but i think there's some lower ones that wouldnt.
	#if defined(DIFFUSEMAP_x2)
	if(texCoord.x <= 0.5f) 
	{
		corrTC = float2(texCoord.x * 2.0f, texCoord.y);
   		colDiffMap = tex2D(diffuseSampler, corrTC).rgba;
   		colGloss = tex2D(diffuseSampler, corrTC).a;	
	}
	else 
	{
		corrTC = float2((texCoord.x - 0.5f) * 2.0f, texCoord.y);
   		colDiffMap = tex2D(diffuseSampler2, corrTC).rgba;
   		colGloss = tex2D(diffuseSampler2, corrTC).a;	
	}
	#else
	corrTC = texCoord; 
   	colDiffMap = tex2D(diffuseSampler, corrTC).rgba;
   	colGloss = tex2D(diffuseSampler, corrTC).a;
	#endif
	
	// Accumulators and intermediate dp3 variable
	float3 colDiffuse = 0.0f;
	float lightIntensity = 1.0f;
	
	// Lambertian diffuse term (N dot L)
	#if defined(NORMALMAP) // TODO: if not NORMALMAP defined this fails. dur!  need alt path.  But added this #if defined(NORMALMAP) but don't know if the lightIntensity of 1.0f in other case is correct
	// TODO: following is wrong.  we must still compute lightIntensity using vertex normal _IF_ no normal map is provided
	lightIntensity = saturate(dot(diffNormal, lightVec)); 
	#endif
	colDiffuse = lightIntensity * lightCol * colDiffMap.rgb;
	

	#if defined(EMISSIVEMAP)
	float3 colEmissive = tex2D(sampEmissive, texCoord).rgb;
	float3 colEmis = colEmissive * pow((1 - lightIntensity), 2) * 0.4f;
	#else
	float3 colEmis = float3(0,0,0);
	#endif 
	
	float colSpecular = 0.0f;
	#if defined(NORMALMAP)  // this is arithmetic specular without using specular data
	// Specular highlightning term
	// - http://www.gamasutra.com/view/feature/131275/implementing_lighting_models_with_.php?page=2
	lightIntensity = saturate(dot(reflectVec, viewVec));
	// Power of that term
	// Arithmetic, pow()-using version
	colSpecular = pow(lightIntensity, SPEC_POW_INTERVAL);
	colSpecular *= colGloss * 0.5f + 0.25f;
	colSpecular *= 0.2f;
	#endif
	// Sum of it all...
	float3 retColor = saturate(colDiffuse + colSpecular + colEmis); 

	// alpha for
	// clouds rendering.  Also we should allow for option to vary the opacity for a cloud layer... if not
	// a parameter, then a calculation like the old cloudshader used which used dotproduct light intensity x hardcode val
	//IN.pos_clip = log(C * IN.pos_clip.z + 1) / log(C * far + 1) * IN.pos_clip.w;
    return float4(retColor, colDiffMap.a);
	
	// // Specular reflection vector for Phong
	// // MSDN	->		L - 2 * (N dot L) * N
	// // W.Engel ->	2 * (N dot L) * N - L
	// float3 reflectVec;
	// // Using W.Engel...
	// reflectVec = 2 * dot(specNormal, lightVec) * specNormal - lightVec;
	// reflectVec = normalize(reflectVec);
	
	// float3 colDiffMap = tex2D(diffuseSampler, IN.uv).rgb;
	
	// // Accumulators and intermediate dp3 variable
	// float3 colDiffuse = 0.0f;
	// float colSpecular = 0.0f;
	// float dotProduct;
	
	// // Lambertian diffuse term (N dot L)
	// dotProduct = dot(diffNormal, lightVec);
	// colDiffuse = dotProduct * light0Col * colDiffMap.rgb * 0.95f;
	
	// // Specular highlightning term
	// dotProduct = saturate(dot(reflectVec, viewVec));
	// // Power of that term
	// // Arithmetic, pow()-using version
	// colSpecular = pow(dotProduct, SPEC_POW_INTERVAL) * 0.1f;
	
	// // Sum of it all...
	// return float4(saturate(colDiffuse + colSpecular), 1.0f);
}

// float4  ps_forward(in PS_INPUT input) : COLOR0
// {
	 // float2 newUV = input.uv;
	 // #if PARALLAX == 1
	 // float3 view_vec = normalize(input.tangent_space_view);
	 // float height = tex2D(bumpSampler, input.uv).a;
     // height *= scale_bias.x - scale_bias.y;
	 // //offset the UV so below we select normals from bumpSampler on those modified UVs
	 // newUV += (height * view_vec.xy); 
	 // #endif
	 
	 // float3x3 tbn_world  = float3x3(input.t, input.b, input.n); 	 
	 // float3 tangent_space_lightvec = mul(tbn_world, (LightPos - (float3)input.pos_world));
	 
	 // float3  lightdir = normalize(tangent_space_lightvec);
	 // float3  pixel_normal = 2.0f * tex2D(bumpSampler, newUV).rgb - 1.0f;	
	 // float   diffuse_lighting = saturate(dot(pixel_normal, lightdir) * 0.5f + 0.5f) * clamp(1 - length(input.pos_world) / Radius, 0, 1);
	 
	 // #if defined (SPECULAR)
	 // float   specular_lighting = CalculateLyon(view_vec, lightdir, pixel_normal);
	 // #else
	 // float   specular_lighting = 0; 
	 // #endif
	 
	 // float4  pixel = diffuse_lighting * float4(tex2D(diffuseSampler, newUV).rgb, 1) + specular_lighting; // * input.Color; // input.Color is minimesh if .ColorArray is provided
	 // return  pixel;
// }
#endif



// -------------------------------------------------------------
// Technique
// -------------------------------------------------------------
technique primary 
{
    pass pass0 
	{
        VertexShader = compile vs_2_0 VS();
		#if defined(DEFERRED)
        PixelShader  = compile ps_2_0 ps_deferred();
		#else
		PixelShader  = compile ps_2_0 ps_forward();
		#endif
    }
}