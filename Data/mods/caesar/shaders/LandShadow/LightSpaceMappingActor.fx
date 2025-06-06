///////////////////////////////////////////////////
// Example file for custom actor shaders using GPU
///////////////////////////////////////////////////





// a matrix here is only using 3 rows (float4x3) so we will use an array of float4x3, it MUST have the semantic BONES
float4x3 boneMatrices[52] : BONES;

// here is the number of bones used per vertex 
int iNumBonePerVertex : BONESPERVERTEX;  

// -------------------------------------------------------------
// Those settings are (and must be) set by the host program
// -------------------------------------------------------------
//#define DEPTH_TESTING
//#define SHADOW_PROJECTION
//#define USE_VERTEX_COLOR
//#define TRANSPARENCY

// -------------------------------------------------------------
// Semantic mappings
// -------------------------------------------------------------
float4x4 worldMat : WORLD;
float4x4 ViewProj : VIEWPROJECTION;
#ifdef SHADOW_PROJECTION
	#ifndef USE_VERTEX_COLOR
		float4 materialDiffuse : DIFFUSE;
	#endif
#endif

// -------------------------------------------------------------
// Constants
// -------------------------------------------------------------
#define ALPHA_TEST_REFERENCE_VALUE 0.5f

// -------------------------------------------------------------
// Textures & Samplers
// -------------------------------------------------------------
texture texture0 : TEXTURE0; // tv will automatically set current texture to TEXTURE0
sampler sampler1 = sampler_state 
{
	Texture = (texture0);
};


// -------------------------------------------------------------
// Input/Output channels
// -------------------------------------------------------------

// non bumpmapped version, so there is no Tangent nor Binormal vector :
struct VS_INPUT_NONBUMP
{
	float3 position : POSITION ;    // vertex position in the default pose
   	float3 normal : NORMAL;	   // vertex normal in the default pose
   	float4 blendindices : BLENDINDICES;  // indices of the 4 bone indices used in the bone matrix array
   	float4 blendweights : BLENDWEIGHT;	// weights of each bone.
   	float2 tex1 : TEXCOORD0;		// standard texture coordinates.
	#ifdef SHADOW_PROJECTION
		#ifdef USE_VERTEX_COLOR
			float4 vertexColor : COLOR;
		#endif
	#endif
};

// bumpmapped version, with tangents :
struct VS_INPUT_BUMP
{
	float3 position : POSITION;
	float3 normal : NORMAL;		//
	float3 tangent : TANGENT;	//  this forms the tangent matrix.
        float3 binormal : BINORMAL; 	//
	float4 blendindices : BLENDINDICES;
	float4 blendweights : BLENDWEIGHT;
	float2 tex1 : TEXCOORD0;
	#ifdef SHADOW_PROJECTION
		#ifdef USE_VERTEX_COLOR
			float4 vertexColor : COLOR;
		#endif
	#endif
};

// simple sample that does nothing fancy.
struct VS_OUTPUT
{
   float4 position : POSITION;
   float4 diffuse : COLOR0;
   float2 tex : TEXCOORD0;  
   #ifdef SHADOW_PROJECTION
	float alpha : TEXCOORD2;
   #endif 
};

// here is the standard skinning function of TV to skin the position and normal
// there must be 5 versions : one for each case (0 bone blending, 1 bone blending, 2 bones blending, .. ,4 bones blending)
void TransPositionNormal( uniform int iNumBones, float4 modelPos, float3 modelNorm, 
                   float4 boneWeights, float4 fBoneIndices, out float3 pos, out float3 norm)
{

	if(iNumBones == 0)
	{
  	        pos = mul(modelPos, worldMat);
		norm = mul(modelNorm, (float3x3)worldMat);
	}
	else
	{
        	int4 ibone = D3DCOLORtoUBYTE4(fBoneIndices);
	        int   IndexArray[4]        = (int[4])ibone;

		pos = 0;
		norm = 0;
		float lastweight = 0.0f;
		for(int i = 0; i < iNumBones - 1; i++)
		{
	           float4x3 mat = boneMatrices[IndexArray[i]]; 
		   lastweight = lastweight + boneWeights[i];
		   pos += mul(modelPos, mat) *  boneWeights[i];
		   norm += mul(modelNorm, (float3x3)mat ) *  boneWeights[i];
		}		
		
		lastweight = 1 - lastweight;
   		float4x3 mat = boneMatrices[IndexArray[iNumBones-1]]; 
        	pos += mul(modelPos, mat) * lastweight;
		norm += mul(modelNorm, (float3x3)mat ) *  lastweight;
	}
	return;

}  



// -------------------------------------------------------------
// Vertex Shader
// -------------------------------------------------------------
VS_OUTPUT ActorShader(uniform int iNumBones, VS_INPUT_NONBUMP inp)
{
   VS_OUTPUT o = (VS_OUTPUT)0;
   float3 pos, norm;
   
   // use our skinning method 
   TransPositionNormal( iNumBones, float4(inp.position, 1.0f), inp.normal, inp.blendweights, inp.blendindices, pos, norm);
   
   // then let's transform the position into clip space using view proj matrix
   o.position = mul(float4(pos,1.0f), ViewProj);
  
   o.tex = inp.tex1;
   
   #ifdef SHADOW_PROJECTION
	#ifdef USE_VERTEX_COLOR
		o.alpha = inp.vertexColor.a;
	#else
		o.alpha = materialDiffuse.a;
	#endif
   #endif
   return o;
}


// -------------------------------------------------------------
// Pixel Shader
// -------------------------------------------------------------
float4 ActorPixelShader(VS_OUTPUT inp) : COLOR0
{
   #ifdef DEPTH_TESTING
	#ifdef TRANSPARENCY
		clip(tex2D(sampler1, inp.tex).a - ALPHA_TEST_REFERENCE_VALUE);
	#endif
	return float4(inp.depth.rrr, 1);
   #endif

   #ifdef SHADOW_PROJECTION
	#ifdef TRANSPARENCY
		return float4(0.0f.rrr, tex2D(sampler1, inp.tex).a * inp.alpha);
	#else
		return float4(0.0f.rrr, 1);
	#endif
   #endif

   return 1;
}

VertexShader VSArray[5] = { compile vs_2_0 ActorShader(0),
			    compile vs_2_0 ActorShader(1),
			    compile vs_2_0 ActorShader(2),
   			    compile vs_2_0 ActorShader(3),
	 		    compile vs_2_0 ActorShader(4)
			  };


// -------------------------------------------------------------
// Techniques
// -------------------------------------------------------------
technique ActorShader
{
    pass pass0
    {
       VertexShader = ( VSArray[iNumBonePerVertex] );
       PixelShader = compile ps_2_0 ActorPixelShader();
       Texture[0] = <texture0>;
    }
}
