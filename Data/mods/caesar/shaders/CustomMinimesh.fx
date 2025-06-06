//
//   Minimesh custom shader example
//
//


// we need an array of matrices
// 	max : 52 matrices for vS2.0
//	      16 matrices for VS1.1
// this array must have the semantic : MINIMESH_WORLD
// it can be 4x3 or 4x4
float4x3 minimeshes[52] : MINIMESH_WORLD;

// we can have the colors too
float4 minimeshes_color[52] : MINIMESH_COLOR;
// only used when MiniMesh.SetColorMode is enabled.

float4 minimeshes_fade : MINIMESH_FADE;
// fading options from the engine.
// VECTOR4(fFarDistance, fStartFading, fMaxAlpha, fMaxAlpha/(fStartFading - fFarDistance));
// the last parameter can obviously be used to optimize the fade computation as seen in the example below.		

float3 CameraPos : CAMERAPOSITION;
float3 CameraDir : CAMERADIRECTION; 

// the usual view projection matrix 
float4x4 viewProj : VIEWPROJECTION;

// our texture used on the minimesh
texture tex : TEXTURE0;
sampler samp1 : register(s0);

struct VS_STANDARD_VERTEX
{
    float4 position : POSITION;
    float3 normal : NORMAL;
    float2 tex : TEXCOORD0;
    float2 index : TEXCOORD3; // WARNING ! THIS HAS CHANGED FROM OLD VERSIONS
    													// NOW YOU HAVE TO USE TEXCOORD3 FOR INDEXING !!
};

struct VS_OUTPUT
{
    float4 position : POSITION;
    float2 tex : TEXCOORD0;
    float4 color : COLOR0;
};

VS_OUTPUT VS_MiniMesh(VS_STANDARD_VERTEX vertex)
{
	VS_OUTPUT o;
	
	// the index of the minimesh matrix to use
	// is in index.x, let's use it !
	float4x3 ourMatrix = minimeshes[vertex.index.x];

	// now we can just multiply the vertex by this matrix
	// and by the view projection	
	float3 worldPos = mul(vertex.position, ourMatrix);
	
	// here of course we can do some vertex modifications
	// with wind etc.
	o.position = mul(float4(worldPos,1.0f), viewProj);


	// we transmit the texture coordinates to the pixel shader
	o.tex = vertex.tex;

	// now we choose the good color 
	// be sure to have called SetColorMode(true) and set good colors with non-null alpha
	// for the minimeshes.
	o.color = minimeshes_color[vertex.index.x];

	// if you don't want to deal with colors, just use that: 
        //o.color = float4(1,1,1,1);

	// handle the fading directly in vertex shader.
  	// float dist = dot(p-CameraPos,CameraDir);
	// o.color.alpha = saturate( (dist - mMiniMesh.x) * mMiniMesh.w);


	return o;
} 

// a very simple pixel shader to go with this vertex shader.
float4 PS_MiniMesh(VS_OUTPUT input) : COLOR0
{
	return input.color * tex2D(samp1,input.tex);
}


technique minimesh
{
	pass pass0
	{
		VertexShader = compile vs_2_0 VS_MiniMesh();
		PixelShader = compile ps_1_1 PS_MiniMesh();
	}
} 