// -------------------------------------------------------------
// MATRIX PARAMETERS
// -------------------------------------------------------------
float4x4 ViewProjection 	: VIEWPROJECTION;
float3 g_ViewPosition 		: VIEWPOSITION;

// -------------------------------------------------------------
// INSTANCE PARAMETERS
// -------------------------------------------------------------
#define InstancesPerBatch 224 // 248
float4 InstancePositionsWS[InstancesPerBatch] : register(c32); // c8 ??? 256 - InstancesPerBatch == 8?
// float3 InstanceScales[InstancesPerBatch];
// float3 InstanceRotations[InstancesPerBatch]; // NOTE: if only Y rotation is allowed, w parameter in InstancePositionWS contains Phi in radians.  For complete rotations, we must use seperate InstanceRotations.
// float2 InstanceUVs[InstancesPerBatch];

// -------------------------------------------------------------
// TEXTURES & SAMPLERS
// -------------------------------------------------------------
textureCUBE textureDiffuse : TEXTURE; // NOTE: TV3D TEXTURE0 semantic not available since this is not a TVMesh. 
samplerCUBE diffuseSampler = sampler_state 
{
	Texture = <textureDiffuse>;
	MinFilter = ANISOTROPIC; // LINEAR
	MagFilter = POINT; //??? LINEAR
	//MipFilter = POINT;
};

// -------------------------------------------------------------
// INPUT / OUTPUT STRUCTS
// -------------------------------------------------------------
struct VS_INPUT
{
// TODO: why are Position and Normal float4 and not float3?  The vertex declaration
// in code is Vector3 for both. Is it because unlike TV3D, d3d fills them
// as float4? 
	float4 PositionMS 		: POSITION;
	float4 NormalOS 		: NORMAL;		
	float InstanceIndex 	: TEXCOORD3;
};

struct VS_OUTPUT 
{
	// NOTE: cube texture has u,v,w parameters thus float3 
	float3 TextureCoordinate : TEXCOORD;
};

// -------------------------------------------------------------
// FUNCTIONS
// -------------------------------------------------------------

// GetInstanceMatrix(float4)
// - assumes w parameter of instancePositionPhi includes a Y axis rotation value in radians.
float4x4 GetInstanceMatrix (float4 instancePositionPhi)
{
	float sinPhi, cosPhi;
	// TODO: since our rotations for terrain tends to be in 90 degree increments
	// we should be able to use a simple lookup instead of computing sin/cos
	sincos(instancePositionPhi.w, sinPhi, cosPhi);
	float4x4 instanceMatrix = 
	{
		cosPhi,	0,	-sinPhi,		0,
		0,		1,	0,				0,
		sinPhi,	0,	cosPhi,			0,
		instancePositionPhi.xyz,	1 
	};
	return instanceMatrix;
}

// -------------------------------------------------------------
// VERTEX SHADER - TECHNIQUE 'OCCLUSION'
// -------------------------------------------------------------
void VS_Occlusion(VS_INPUT In, out float4 PositionCS : Position)
{
	int instanceIndex = (int)In.InstanceIndex;
	float4 instancePositionWS = float4(InstancePositionsWS[instanceIndex].xyz, 0);
	// NOTE: since these are perfect square blocks, and rotations are limited to 90degree Y axis only
	//       there is no need to compute instanceMatrix as we do in the 'SHADOWED' technique. 
	//       But keep this in mind, once we switch to different types of objects, the occlusion function
	//       must be modified or a different branch selected or it wont work properly.
	// NOTE: no scaling supported here, but not having to compute a world matrix for each element
	// on CPU and pass to shader is much faster.  
	PositionCS = mul(In.PositionMS + instancePositionWS, ViewProjection);
}

// -------------------------------------------------------------
// VERTEX SHADER - TECHNIQUE 'SHADOWED'
// -------------------------------------------------------------
VS_OUTPUT VS_Shadowed(VS_INPUT IN, out float4 PositionCS : Position)
{
	VS_OUTPUT OUT;

	int instanceIndex = (int)IN.InstanceIndex;

	float4x4 instanceMatrix = GetInstanceMatrix (InstancePositionsWS[instanceIndex]);
	float4 positionWS = mul(IN.PositionMS, instanceMatrix);
	PositionCS = mul(positionWS, ViewProjection);
	
	// PositionMS maps to u,v,w parameters of cube texture
	// NOTE: if the model space coords are not for a unit cube mesh
	// then the z position parameter will not map properly to w cube texture lookup
	// In other words, we would need dedicated UV's for other types of mesh or
	// not use the cube texture.  For distant LOD, cube texture seems good idea
	// but for near camera rendering for our purposes probably not.
	OUT.TextureCoordinate = IN.PositionMS;
	return OUT;
}

// -------------------------------------------------------------
// PIXEL SHADER - TECHNIQUE 'OCCLUSION'
// -------------------------------------------------------------
float4 PS_Occlusion() : COLOR 
{
	return 1; // unshaded white pixel
}

// -------------------------------------------------------------
// PIXEL SHADER - TECHNIQUE 'SHADOWED'
// -------------------------------------------------------------
float4 PS_Shadowed(VS_OUTPUT IN) : COLOR 
{
	// TODO: texCUBE could be a good LOD texturing option.
	return texCUBE(diffuseSampler, IN.TextureCoordinate);	
}

// -------------------------------------------------------------
// TECHNIQUES
// -------------------------------------------------------------	
technique shadowed
{
	pass pass0 // shadowed
	{
		VertexShader = compile vs_3_0 VS_Shadowed();
		PixelShader  = compile ps_3_0 PS_Shadowed();    
	}
	
	pass pass1 //occlusion
	{
		VertexShader = compile vs_3_0 VS_Occlusion();
		PixelShader  = compile ps_3_0 PS_Occlusion();    	
	}
}