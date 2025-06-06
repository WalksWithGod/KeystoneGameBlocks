//
//   Minimesh shader - Textured and with Specular lighting using one Directional Light
//

// we need an array of matrices
// 	max : 52 matrices for vS2.0
//	      16 matrices for VS1.1
// this array must have the semantic : MINIMESH_WORLD
// it can be 4x3 or 4x4
float4x3 minimeshes[52] : MINIMESH_WORLD;

// we can have the colors too
//float4 minimeshes_color[52] : MINIMESH_COLOR; 
// only used when MiniMesh.SetColorMode is enabled.

float3 CameraPos : CAMERAPOSITION;
float3 CameraDir : CAMERADIRECTION; 
float3 View : VIEW;
// the usual view projection matrix 
float4x4 viewProj : VIEWPROJECTION;

// light
float3 dirLightColor : LIGHTDIR0_COLOR; 
float3 dirLightDir : LIGHTDIR0_DIRECTION;

// material lighting properties
float4 materialDiffuse : DIFFUSE; 
float3 materialEmissive : EMISSIVE;
float3 materialAmbient : AMBIENT; 
float3 materialSpecular : SPECULAR; 
float materialPower : SPECULARPOWER; 


texture diffuseTexture : TEXTURE0;

sampler diffuseSampler = 
sampler_state
{
	Texture = (diffuseTexture);
};


struct VS_INPUT
{
    float4 position : POSITION;
    float3 normal : NORMAL;
	float2 texCoord : TEXCOORD0;
    float2 index : TEXCOORD3; // Minimesh Element Index is ALWAYS on TEXCOORD3.
};

struct VS_OUTPUT
{
    float4 outPosition : POSITION;
	float4 newPosition : TEXCOORD0;
	float3 normal : NORMAL;
    //float4 color : COLOR0;
	float2 texCoord : TEXCOORD1;
};

VS_OUTPUT VS_MiniMesh(VS_INPUT IN)
{
	VS_OUTPUT OUT;
	
	// the index of the minimesh matrix to use
	// is in index.x, let's use it !
	float4x3 ourMatrix = minimeshes[IN.index.x];

	// now we can just multiply the vertex by this matrix and by the view projection	
	float3 worldPos = mul(IN.position, ourMatrix);
	
	// here of course we can do some vertex modifications with wind etc.
	OUT.outPosition = mul(float4(worldPos,1.0f), viewProj);
	OUT.newPosition = OUT.outPosition;
	
	//calc normal vec
	OUT.normal = mul(ourMatrix,IN.normal);
		
	OUT.texCoord = IN.texCoord;
	// now we choose the good color 
	// be sure to have called SetColorMode(true) and set good colors with non-null alpha
	// for the minimeshes.
	//OUT.color = minimeshes_color[IN.index.x];

	// if you don't want to deal with colors, just use that: 
        //OUT.color = float4(1,1,1,1);

	// OUT.color.alpha = saturate( (dist - mMiniMesh.x) * mMiniMesh.w);

	return OUT;
} 

// a simple pixel shader.
float4 PS_MiniMesh(VS_OUTPUT IN) : COLOR0
{
	
	float3 light = normalize(-dirLightDir);
	//float3 v = normalize(CameraPos);
	float3 normal = normalize(IN.normal);

	//Calc half vector average of the light and the view vectors
	float3 halfway = normalize(light + View - IN.newPosition);
	float3 emissive = materialEmissive;
	float3 ambient = materialAmbient;

	//calc diffuse reflection
	float3 diffuse = saturate(dot(normal,light)) * materialDiffuse.rgb;

	//calc specular reflection
	float3 specular = pow(saturate(dot(normal,halfway)),materialPower) * materialSpecular;

	float2 texCoord = IN.texCoord;
	float4 texColor = tex2D(diffuseSampler,texCoord);
	
	//combine all the color components. NOTE: This is not (yet) using the Minimesh[index].Color assigned
	float3 color = (saturate(ambient + diffuse) * texColor + specular) * dirLightColor + emissive;

	//float3 color = (saturate(ambient + diffuse) + specular) * dirLightColor + emissive;

	return float4(color, 1.0f);
}


technique minimesh
{
	pass P0
	{
        VertexShader = compile vs_3_0 VS_MiniMesh();
        PixelShader  = compile ps_3_0 PS_MiniMesh();
    }
} 