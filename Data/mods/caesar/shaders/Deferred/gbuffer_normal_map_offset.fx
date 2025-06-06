// ----------------
// #define FORWARD 
// ------------------------------------------------------------------------------------
// - 
// ------------------------------------------------------------------------------------
// - or - 
// ------------------------------------------------------------------------------------
// #define DEFERRED
// ------------------------------------------------------------------------------------
// Deferred Rendering Geometry Buffer Render Pass Shader for TV3D
// for TVMesh, TVActor and TVMinimesh
// - Requires at least 3 (optionally 4) Render Targets
// - Requires Diffuse Texture Map TEXTURE0
// - Requires NormalMap Texture Map TEXTURE1
//	 optional - Parallax Heightmap in Alpha Channel of NormalMap Texture
//   Optional - Emissive map in Alpha channel of NormalMap Texture 
//   Optional - Specular Mask in Alpha Channel of NormalMap Texture
// -   by Hypnotron Nov.2011 
// -   based on Caitalin's shader, and the TV3D 6.5 official sample shaders
//   http://www.catalinzima.com/tutorials/deferred-rendering-in-xna/custom-content-processor-and-normal-mapping/
// 
// ------------------------------------------------------------------------------------
// IMPORTANT: When declaring and assigning a define there IS NO EQUAL SIGN and NO SEMICOLON at end
// #define GEOMETRY 0 // for TVMesh - TVShader.AddDefine ("Geometry", "0")
// #define GEOMETRY 1 // for TVActor - TVShader.AddDefine ("Geometry", "1")
// #define GEOMETRY 2 // for TVMinimesh - TVShader.AddDefine ("Geometry", "2")
//                    -- Then TVShader.CreateEffectFromFile(path)
// ----------------
// #define NORMALMAP
// 		#define PARALLAX_IN_NORMALMAP_ALPHA  // requires NORMALMAP 
// #define DIFFUSEMAP
// 		#define SPECULAR_IN_DIFFUSEMAP_ALPHA  // requires diffuse map
// #define SPECULARMAP 
// #define EMISSIVEMAP
// ----------------
// #define MATERIAL // uses diffuse, ambient, specular, emissive material properties and opacity.  not compatible with DEFERRED
// ----------------
//
// Helpful Links
// http://msdn.microsoft.com/en-us/library/windows/desktop/bb944006(v=vs.85).aspx
// http://www.neatware.com/lbstudio/web/hlsl.html 
// ------------------------------------------------------------------------------------

// -------------------------------------------------------------
// Semantics
// -------------------------------------------------------------
float4x4 object_to_clip: WORLDVIEWPROJECTION;
float4x4 viewproj  : VIEWPROJECTION;
float3   view  : VIEWPOS;
#if GEOMETRY == 0 || GEOMETRY == 1  // TVMesh || TVActor
float4x4 world  : WORLD;
#endif
#if GEOMETRY == 1                   // TVActor
float4x3 boneMatrices[52] : BONES;
int iNumBonePerVertex : BONESPERVERTEX;  
#elif GEOMETRY == 2                   // TVMinimesh
float4x4 world[52] : MINIMESH_WORLD;
//float4   ElementColor[52]: MINIMESH_COLOR;
#endif

// -------------------------------------------------------------
// Other Parameters
// -------------------------------------------------------------
float Radius = 50.0f;
float3 LightPos; // = float3(0,0,0);
float material_index;   // user can supply a materialIndex which will get stored in one of the output render targets
                       // this materialIndex in the lighting shader will then be read, and used to perform a lookup 
					   // within a volume texture that contains all material properties
					   // An alternative method is to use the materialIndex to allow the light shader
					   // to determine which lighting formula to use in a huge conditional branch.
#if defined(PARALLAX_IN_NORMALMAP_ALPHA)
// x component = parallax scale
// y component = parallax bias
float2 scale_bias = float2(0.04f, 0.02f);
#endif

// -------------------------------------------------------------
// Textures & Samplers
// -------------------------------------------------------------
texture  diffuseMap    : TEXTURE0;
texture  bumpMap      : TEXTURE1;

sampler2D diffuseSampler = sampler_state
{
	 Texture   = (diffuseMap);
	 MIPFILTER = Anisotropic;
	 MAGFILTER = Anisotropic;
	 MINFILTER = Anisotropic;
};

sampler2D bumpSampler = sampler_state
{
	 Texture   = (bumpMap);
	 MIPFILTER = Anisotropic;
	 MAGFILTER = Anisotropic;
	 MINFILTER = Anisotropic;
};

// -------------------------------------------------------------
// Input/Output channels
// -------------------------------------------------------------
struct vs_in
{
	 float4 pos_obj : POSITION;
	 float3 t       : TANGENT;
	 float3 b       : BINORMAL;
	 float3 n       : NORMAL;
	 float2 uv      : TEXCOORD0;
	 #if GEOMETRY == 2 // minimesh
	 float2 index   : TEXCOORD3;
	 #elif GEOMETRY == 1 // actor
	 float4 blendindices : BLENDINDICES;
	 float4 blendweights : BLENDWEIGHT;
	 #endif
};

struct ps_in 
{
	float4 pos_clip : POSITION0;
	float4 pos_world : TEXCOORD0;
	float2 uv : TEXCOORD1;
	float3 t  : TEXCOORD2;
	float3 b  : TEXCOORD3;
	float3 n  : TEXCOORD4;
	#if defined(PARALLAX_IN_NORMALMAP_ALPHA)
	float3 tangent_space_view : TEXCOORD5;
	#endif
};

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
// Support functions
// -------------------------------------------------------------
#if GEOMETRY == 1  // TVActor
// Standard TVActor skinning function to skin position and normal
void TransPositionNormal( uniform int iNumBones, float4 modelPos, float3 modelNorm, float3 modelBinormal, float3 modelTangent,
                   float4 boneWeights, float4 fBoneIndices, out float3 pos, out float3 norm, out float3 binormal, out float3 tangent)
{

	if(iNumBones == 0)
	{
  	    pos = mul((float3)modelPos, (float3x3)world); // Hypnotron Nov.22.2011 changed to (float3)modelPos.  Remove this comment if all works fine.
		norm = mul(modelNorm, (float3x3)world);
		tangent = mul(modelTangent, (float3x3)world);
		binormal = mul(modelBinormal, (float3x3)world);
	}
	else
	{
        int4 ibone = D3DCOLORtoUBYTE4(fBoneIndices);
	    int   IndexArray[4]        = (int[4])ibone;

		pos = float3(0,0,0);
		norm = float3(0,0,0);
		tangent = float3(0,0,0);
		binormal = float3(0,0,0);
		float lastweight = 0.0f;
		for(int i = 0; i < iNumBones - 1; i++)
		{
	       float4x3 mat = boneMatrices[IndexArray[i]]; 
		   lastweight = lastweight + boneWeights[i];
		   pos += mul(modelPos, mat) *  boneWeights[i];
		   norm += mul(modelNorm, (float3x3)mat ) *  boneWeights[i];
		   tangent += mul(modelTangent, (float3x3)mat ) *  boneWeights[i];
		   binormal += mul(modelBinormal, (float3x3)mat ) *  boneWeights[i];
		   
		}		
		
		lastweight = 1 - lastweight;
   		float4x3 mat = boneMatrices[IndexArray[iNumBones-1]]; 
        pos += mul(modelPos, mat) * lastweight;
		norm += mul(modelNorm, (float3x3)mat ) *  lastweight;
		tangent += mul(modelTangent, (float3x3)mat ) *  lastweight;
		binormal += mul(modelBinormal, (float3x3)mat ) *  lastweight;
	}
	return;
}  
#endif

// -------------------------------------------------------------
// Vertex Shader functions
// -------------------------------------------------------------
#if GEOMETRY == 1 // TVActor
ps_in vs_main(uniform int iNumBones, vs_in input)
{
    ps_in output = (ps_in)0;
    float3 pos, norm, binormal, tangent; 
   
    // skinning method 
    TransPositionNormal( iNumBones, input.pos_obj, 
		input.n, input.b, input.t, input.blendweights, 
		input.blendindices, pos, norm, binormal, tangent);
       
    output.pos_clip = mul(float4(pos,1.0f), viewproj);
    output.pos_world = mul(float4(pos,1.0f), world);

	// TransPositionNormal function already converts to world using 
	// world matrix for each bone.  No need to do that here
    //output.t = mul(input.t, world_matrix); 
	//output.b = mul(input.b, world_matrix);
	//output.n = mul(input.n, world_matrix);
	output.t = normalize(tangent);
	output.b = normalize(binormal);
	output.n = normalize(norm);
	output.uv = input.uv;
   
    #if defined(PARALLAX_IN_NORMALMAP_ALPHA)
	// compute tangent space view vector
	float3 eye = view - output.pos_world;
	output.tangent_space_view = mul(float3x3(output.t, output.b, output.n), eye);
	#endif
    return output;
}
#endif

#if GEOMETRY != 1  // TVMesh and TVMinimesh
ps_in vs_main(in vs_in input)
{
	ps_in output;
	
	#if GEOMETRY == 2 // TVMinimesh
    float4x3 world_matrix  = (float4x3)world[input.index.x];
	float4   world_pos     = float4(mul(input.pos_obj, world_matrix),1);  
	output.pos_clip  =  mul(world_pos, viewproj); 
	output.pos_world = world_pos;   
	#else
	float4x3 world_matrix = (float4x3)world;
	output.pos_clip  = mul(input.pos_obj, object_to_clip); 
	output.pos_world = mul(input.pos_obj, world);   
	#endif
	
	// normalize?
	input.t = normalize(input.t);
	input.b = normalize(input.b);
	input.n  = normalize(input.n);
	// per vertex tbn to world, no need for per pixel
	output.t = mul(input.t, (float3x3)world_matrix);
	output.b = mul(input.b, (float3x3)world_matrix);
	output.n = mul(input.n, (float3x3)world_matrix);
	output.uv = input.uv;

	#if defined(PARALLAX_IN_NORMALMAP_ALPHA)
	// compute tangent space view vector
	// don't normalize tangent space view vector in vertex shader
	float3 eye =  view - (float3)output.pos_world;
	output.tangent_space_view = mul(float3x3(output.t, output.b, output.n), eye);
	#endif
	return output;
}
#endif


// -------------------------------------------------------------
// Pixel Shader functions
// -------------------------------------------------------------
#if defined(DEFERRED)
mrt_pixels ps_deferred(in ps_in input)
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
#endif

#if defined(FORWARD)
float4  ps_forward(in ps_in input) : COLOR0
{
	 float2 newUV = input.uv;
	 #if defined(PARALLAX_IN_NORMALMAP_ALPHA)
	 float3 view_vec = normalize(input.tangent_space_view);
	 float height = tex2D(bumpSampler, input.uv).a;
     height *= scale_bias.x - scale_bias.y;
	 //offset the UV so below we select normals from bumpSampler on those modified UVs
	 newUV += (height * view_vec.xy); 
	 #endif
	 
	 float3x3 tbn_world  = float3x3(input.t, input.b, input.n); 	 
	 float3 tangent_space_lightvec = mul(tbn_world, (LightPos - (float3)input.pos_world));
	 
	 float3  lightdir = normalize(tangent_space_lightvec);
	 float3  pixel_normal = 2.0f * tex2D(bumpSampler, newUV).rgb - 1.0f;	
	 float   diffuse_lighting = saturate(dot(pixel_normal, lightdir) * 0.5f + 0.5f) * clamp(1 - length(input.pos_world) / Radius, 0, 1);
	 
	 #if defined (SPECULAR)
	 float   specular_lighting = CalculateLyon(view_vec, lightdir, pixel_normal);
	 #else
	 float   specular_lighting = 0; 
	 #endif
	 
	 float4  pixel = diffuse_lighting * float4(tex2D(diffuseSampler, newUV).rgb, 1) + specular_lighting; // * input.Color; // input.Color is minimesh if .ColorArray is provided
	 return  pixel;
}
#endif

#if GEOMETRY == 1 // TVActor
VertexShader VSArray[5] = { 
				compile vs_2_0 vs_main(0),
			    compile vs_2_0 vs_main(1),
			    compile vs_2_0 vs_main(2),
   			    compile vs_2_0 vs_main(3),
	 		    compile vs_2_0 vs_main(4)
			  };

#endif

#if defined(DEFERRED)
technique primary
{
	 pass pass0
	 {
	     #if GEOMETRY == 1 // TVActor
		 VertexShader = ( VSArray[iNumBonePerVertex] );
		 #else
		 VertexShader     = compile vs_2_0 vs_main();
		 #endif
		 ZEnable = true;
		 ZWriteEnable = true;
		 CullMode = CCW;
		 PixelShader      = compile ps_2_0 ps_deferred();
	 }
}

technique stencil_target
{
	 pass pass0
	 {
	 	#if GEOMETRY == 1 // TVActor
		 VertexShader = ( VSArray[iNumBonePerVertex] );
		 #else
		 VertexShader     = compile vs_2_0 vs_main();
		 #endif
		 ZEnable = true;
		 ZWriteEnable = true;
		 CullMode = CCW;
		 PixelShader      = compile ps_2_0 ps_deferred();
	 }
}

technique stencil_source
{
	 pass pass0
   	 {
   	 	#if GEOMETRY == 1 // TVActor
		 VertexShader = ( VSArray[iNumBonePerVertex] );
		 #else
		 VertexShader     = compile vs_2_0 vs_main();
		 #endif
		 ZEnable = true;
		 ZWriteEnable = true;
		 CullMode = CCW;
		 PixelShader      = compile ps_2_0 ps_deferred();
 	 }
}
#endif

#if defined(FORWARD)
technique primary
{
	pass pass0
	{
		#if GEOMETRY == 1 // TVActor
		 VertexShader = ( VSArray[iNumBonePerVertex] );
		 #else
		 VertexShader     = compile vs_2_0 vs_main();
		 #endif
		 ZEnable = true;
		 ZWriteEnable = true;
		 CullMode = CCW;
		 PixelShader      = compile ps_2_0 ps_forward();
	}	
}

technique stencil_target
{
	pass pass0
	{
		#if GEOMETRY == 1 // TVActor
		 VertexShader = ( VSArray[iNumBonePerVertex] );
		 #else
		 VertexShader     = compile vs_2_0 vs_main();
		 #endif
		 ZEnable = true;
		 ZWriteEnable = true;
		 CullMode = CCW;
		 PixelShader      = compile ps_2_0 ps_forward();
	}
}
	
technique stencil_source
{
	pass pass0 
	{
		#if GEOMETRY == 1 // TVActor
		 VertexShader = ( VSArray[iNumBonePerVertex] );
		 #else
		 VertexShader     = compile vs_2_0 vs_main();
		 #endif
		 ZEnable = true;
		 ZWriteEnable = true;
		 CullMode = CCW;
		 PixelShader      = compile ps_2_0 ps_forward();
	}
}
#endif
