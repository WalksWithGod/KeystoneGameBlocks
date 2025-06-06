// -------------------------------------------------------------
/// Actor Shader - Textured + Normal Mapping + Shadow Mapping
// -------------------------------------------------------------

// -------------------------------------------------------------
// Compilation switches
// -------------------------------------------------------------
// #define NUM_SPLITS_2
//		- frustum is split into 2 halves
// #define NUM_SPLITS_3
//		- frustum is split into 3 parts
// #define NUM_SPLITS_4
//		- frustum is split into 4 parts

// -------------------------------------------------------------
// Fog Constants
// -------------------------------------------------------------
#define FOG_TYPE_NONE    0
#define FOG_TYPE_EXP     1
#define FOG_TYPE_EXP2    2
#define FOG_TYPE_LINEAR  3 

// -------------------------------------------------------------
// Semantics
// -------------------------------------------------------------
float4x4 worldMat : 	WORLD;
float3 viewPosition :  	VIEWPOSITION;
float4x4 ViewProj : 	VIEWPROJECTION;
// a matrix here is only using 3 rows (float4x3) so we will use an array of float4x3, it MUST have the semantic BONES
float4x3 boneMatrices[52] : BONES;
int iNumBonePerVertex : BONESPERVERTEX;  

// -------------------------------------------------------------
///Light Var's
// -------------------------------------------------------------
float3 g_vLightDirection;
float3 dirLightColor : LIGHTDIR0_COLOR;

// for shadow path we should eventually use below light vars, but currently not
// also the array is designed for 6 pointlights really... 
float3 LightPos[6]; //our light position in object space
float3 LightColors[6]; //our light Colors
float LightRanges[6]; //Light Range
int LightNum = 0;

// -------------------------------------------------------------
///Material Var's
// -------------------------------------------------------------
float4 materialDiffuse :     DIFFUSE;
float4 materialAmbient :     AMBIENT;
float4 materialEmissive :    EMISSIVE;
float3 materialSpecular :    SPECULAR;
float materialPower :        SPECULARPOWER;

// -------------------------------------------------------------
///Fog Var's
// -------------------------------------------------------------
int fogType : FOGTYPE;
float3 fogColor : FOGCOLOR;
float fogDensity : FOGDENSITY;
float fogStart : FOGSTART;
float fogEnd : FOGEND;

// -------------------------------------------------------------
///Shadow Var's
// -------------------------------------------------------------
float4x4 g_mShadowMap[4];
float4x4 g_mLightViewProj[4];
#if defined (NUM_SPLITS_4)
	float4 g_fSplitDistances;
#elif defined (NUM_SPLITS_3)
	float3 g_fSplitDistances;
#elif defined (NUM_SPLITS_2)
	float2 g_fSplitDistances;
#endif

float SHADOW_OPACITY = 0.45f;
float3 ShadowColor = float3(1, 1, 1);
float Bias = 0.001f;

// -------------------------------------------------------------
///Exponential Shadow Map Var's which stores depth info in a scaled way (as opposed to linear)
// TODO: I should make ESM a #define
// -------------------------------------------------------------
float ESM_K = 60.0f; // Range [0, 80]
float ESM_MIN = -0.5f; // Range [0, -oo]
float ESM_DIFFUSE_SCALE = 1.39f; // Range [1, 10]

// -------------------------------------------------------------
/// Textures & Samplers
// -------------------------------------------------------------
texture texShadowMap;
sampler2D sampShadowMap  =
sampler_state
{
	Texture = (texShadowMap);
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = Border;
	AddressV = Border;
	AddressW = Border;
	BorderColor = 0x0000000;
};

texture BaseTex : TEXTURE0;
sampler2D BaseSampler = sampler_state
{
    Texture = <BaseTex>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
};

// here is the non bumpmapped version, so there is no Tangent nor Binormal vector :
struct VS_INPUT_NONBUMP
{
	float3 position : POSITION ;    // vertex position in the default pose
   	float3 normal : NORMAL;	   // vertex normal in the default pose
   	float4 blendindices : BLENDINDICES;  // indices of the 4 bone indices used in the bone matrix array
   	float4 blendweights : BLENDWEIGHT;	// weights of each bone.
   	float2 tex1 : TEXCOORD0;		// standard texture coordinates.
};

// simple sample that does nothing fancy.
struct VS_OUTPUT
{
	float4 position : POSITION; 
	float3 vNormal : TEXCOORD0;
	float3 vDepthAndTextureUV : TEXCOORD1;
	float4 fLightDepth : TEXCOORD2;
	float4 vProjectedLightPosition[2] : TEXCOORD3;
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
/// Vertex Shader
// -------------------------------------------------------------

VS_OUTPUT ActorShader(uniform int iNumBones, VS_INPUT_NONBUMP inp)
{
	VS_OUTPUT OUT;


	// use our skinning method 
	float3 pos, norm; 	// out parameters
	TransPositionNormal( iNumBones, float4(inp.position, 1.0f), inp.normal, inp.blendweights, inp.blendindices, pos, norm);

	float4 positionWS = float4(pos, 1.0f);

	// then let's transform the position into clip space using view proj matrix
	OUT.vViewDir = viewPosition - positionWS;
	OUT.vScreenSpacePosition = mul(positionWS, ViewProj);
    OUT.vNormal = inp.normal;
	OUT.vDepthAndTextureUV.xy = inp.tex1;
	OUT.vDepthAndTextureUV.z = OUT.vScreenSpacePosition.z;
	
	float4 mLightSpace;
	
	OUT.vProjectedLightPosition[0].xy = mul(positionWS, g_mShadowMap[0]).xy;
	OUT.vProjectedLightPosition[0].zw = mul(positionWS, g_mShadowMap[1]).xy;
	OUT.vProjectedLightPosition[1].xy = mul(positionWS, g_mShadowMap[2]).xy;
	OUT.vProjectedLightPosition[1].zw = mul(positionWS, g_mShadowMap[3]).xy;
	
	for(int n = 0; n < 4; n++ )
	{
		mLightSpace =  mul(positionWS, g_mLightViewProj[n]);
		OUT.fLightDepth[n] = mLightSpace.z / mLightSpace.w - Bias;
	}
	return OUT;
}


// -------------------------------------------------------------
/// Functions
// -------------------------------------------------------------

half GetSplitByDepth(float fDepth)
{
	#if defined (NUM_SPLITS_4)
		float4 fTest = fDepth > g_fSplitDistances;
	#elif defined (NUM_SPLITS_3)
		float3 fTest = fDepth > g_fSplitDistances;
	#elif defined (NUM_SPLITS_2)
		float2 fTest = fDepth > g_fSplitDistances;
	#endif
	return dot(fTest, fTest);
}


float3 GetShadow(half fSplitIndex, float2 splitUV[4], float4 lightDepths)
{
	float shadow = 0.0f;
    float receiver = lightDepths[fSplitIndex];
	float occluder = tex2D(sampShadowMap, splitUV[fSplitIndex].xy)[fSplitIndex];
	
	shadow = saturate(exp(max(ESM_MIN, ESM_K * (occluder - receiver))));
	shadow = 1.0f - (ESM_DIFFUSE_SCALE * (1.0f - shadow));
    //if (shadow < 0.8f) shadow = max(ShadowColor, shadow.rrr);
    // return shadow;
	
	// Pixels that are further from the light than the depth
	// in the projected shadow depth buffer are shadowed. 
	float fShadow = receiver > occluder;
	fShadow = 1.0f - fShadow * SHADOW_OPACITY;
	return fShadow;
}

// Diffuse Lighting Models
float Lambert(float3 normal, float3 lightDir)
{
	return saturate(dot(normal, lightDir));
}

float WrapLambert (float3 normal, float3 lightDir) 
{
        float NdotL = dot (normal, lightDir);
        float diff = NdotL * 0.5f + 0.5f;
		return diff;
        // float4 c;
        // c.rgb = s.Albedo * _LightColor0.rgb * (diff * atten * 2);
        // c.a = s.Alpha;
        // return c;
}

float DiffuseLighting (float3 normal, float3 lightDir)
{
	// #ifdef LAMBERT
		// return Lambert(normal, lightDir);
	// #elif defined(WRAP_LAMBERT)
		 return WrapLambert(normal, lightDir);
}

// Phong specular reflection model
float Phong(float3 normal, float3 view, float3 lightDir)
{					
	float3 reflected = reflect(-lightDir, normal);
	return pow(saturate(dot(reflected, view)), materialPower);
}

// Calculates the specular contribution based on the selected model
float Specular(float3 normal, float3 view, float3 lightDir) 
{
	return Phong(normal, view, lightDir);
		// #ifdef PHONG
			// Phong(normal, view, lightDir);
		// #elif defined(BLINN_PHONG)
			// BlinnPhong(normal, view, lightDir);
		// #elif defined(LYON)
			// Lyon(normal, view, lightDir);
		// #elif defined(TORRANCE_SPARROW)
			// TorranceSparrow(normal, view, lightDir);
		// #elif defined(TROWBRIDGE_REITZ)
			// TrowbridgeReitz(normal, view, lightDir);
		// #else
			// 0;
		// #endif	
}

// Calculate the final pixel color using all calculated components
float4 FinalizeColor(float3 diffuse, float3 specular, float2 texCoord) //, float specularMask, float2 vertexFog)
{
	// Constant color contributions
	float4 albedo = tex2D(BaseSampler, texCoord);
	float3 emissive = materialEmissive; // * tex2D(sampEmissive, texCoord).rgb;
	float3 ambient = materialAmbient;    
    			
    // Mask components		
	diffuse *= materialDiffuse.rgb;    
	specular *= materialSpecular; // * specularMask;
	
	// Pixel alpha
	float alpha = albedo.a * materialDiffuse.a;
   
	// Compute output color (with fog)
	float3 color = saturate(ambient + diffuse) * albedo + specular + emissive;     	  
	//color = lerp(color, fogColor, fog);
	//color = color * vertexFog.x + vertexFog.y;
	
    return float4(color, alpha);
}

// -------------------------------------------------------------
/// Pixel Shader
// -------------------------------------------------------------

// simple pixel shader that just output the pixel
float4 ActorPixelShader(VS_OUTPUT IN) : COLOR0
{
    // diffuse texture color of the model
    float4 diffuseColor = tex2D(BaseSampler, IN.vDepthAndTextureUV.xy);
	
    // clip aborts PS if value is negative (but apparently this does not result in early exit
	// because it acts like a branch and the rest of the function occurs anyway and final
	// pixel is kept based on clip result).  Also early-Z optimization is disabled when 
	// using clip.  So ideally you want to only use variant of this shader on meshes that require it.
	clip(diffuseColor.a - 0.5f);
	
	half fSplitIndex = GetSplitByDepth(IN.vDepthAndTextureUV.z);

	float2 SplitUV[4];
	SplitUV[0] = IN.vProjectedLightPosition[0].xy;
	SplitUV[1] = IN.vProjectedLightPosition[0].zw;
	SplitUV[2] = IN.vProjectedLightPosition[1].xy;
	SplitUV[3] = IN.vProjectedLightPosition[1].zw;
	
	
    // Intensity based on the direction of the light
    float diffuseIntensity = DiffuseLighting (IN.vNormal, g_vLightDirection) * dirLightColor;
	float specular = Specular(IN.vNormal, viewPosition, g_vLightDirection) * dirLightColor;
	
    // Final diffuse color with ambient color added
    float4 diffuse = FinalizeColor (diffuseIntensity, specular, IN.vDepthAndTextureUV.xy); 
    
	// get value to lower pixel intensity by depending on whether it's shadowed or not
	float shadow = GetShadow(fSplitIndex, SplitUV, IN.fLightDepth);
		
	// Shadow the pixel by lowering the intensity
    diffuse *= float4(shadow, shadow, shadow, 1.0f);

	return diffuse;
}

VertexShader VSArray[5] = { compile vs_3_0 ActorShader(0),
			    compile vs_3_0 ActorShader(1),
			    compile vs_3_0 ActorShader(2),
   			    compile vs_3_0 ActorShader(3),
	 		    compile vs_3_0 ActorShader(4)
			  };

technique shadowed
{
    pass pass0
    {
       VertexShader = ( VSArray[iNumBonePerVertex] );
       PixelShader = compile ps_3_0 ActorPixelShader();
    }
}

// -------------------------------------------------------------//////////////////
// -------------------------------------------------------------//////////////////
/////Depth Shader:
/////
// -------------------------------------------------------------//////////////////
// -------------------------------------------------------------//////////////////


// here is the non bumpmapped version, so there is no Tangent nor Binormal vector :
struct VS_IN_DEPTH
{
	float3 position : POSITION ;    // vertex position in the default pose
   	float3 normal : NORMAL;	   // vertex normal in the default pose
   	float4 blendindices : BLENDINDICES;  // indices of the 4 bone indices used in the bone matrix array
   	float4 blendweights : BLENDWEIGHT;	// weights of each bone.
   	float2 tex1 : TEXCOORD0;		// standard texture coordinates.
};

// simple sample that does nothing fancy.
struct VS_OUT_DEPTH
{
	float4 position : POSITION; 
	float2 UV : TEXCOORD0;
	float Depth : TEXCOORD1;
};

// -------------------------------------------------------------
/// Vertex Shader
// -------------------------------------------------------------

VS_OUT_DEPTH ActorDShader(uniform int iNumBones, VS_IN_DEPTH inp)
{
	VS_OUT_DEPTH OUT;
	float3 pos, norm;

	// use our skinning method 
	TransPositionNormal( iNumBones, float4(inp.position, 1.0f), inp.normal, inp.blendweights, inp.blendindices, pos, norm);

	// then let's transform the position into clip space using view proj matrix
	OUT.position = mul(float4(pos, 1.0f), ViewProj);
	//Output Depth
	OUT.Depth = OUT.position.z;
	
	OUT.UV = inp.tex1;
	return OUT;
}

// simple pixel shader that just output the pixel
float4 ActorDepthPixelShader(VS_OUT_DEPTH IN) : COLOR0
{
	clip(tex2D(BaseSampler, IN.UV).a - 0.5f);
	return IN.Depth;
}

VertexShader VSDArray[5] = { compile vs_3_0 ActorDShader(0),
			    compile vs_3_0 ActorDShader(1),
			    compile vs_3_0 ActorDShader(2),
   			    compile vs_3_0 ActorDShader(3),
	 		    compile vs_3_0 ActorDShader(4)
			  };

technique depth
{
	pass p0
	{
		AlphablendEnable = false;
		AlphaTestEnable = false;
		VertexShader = ( VSDArray[iNumBonePerVertex] );
		PixelShader = compile ps_3_0 ActorDepthPixelShader();
	}
}