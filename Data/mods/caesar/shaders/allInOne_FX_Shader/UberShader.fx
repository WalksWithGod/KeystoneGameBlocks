#include "TextureSamplers.hlsl"

////////////////////////////////////////////////////////
// Static and Dynamic constants
////////////////////////////////////////////////////////

//#define VS_MODEL vs_2_0
//#define PS_MODEL ps_2_0
#include "LightingFunctions.hlsl"

//---- Matrices ----
float4x4 m_World : World < bool UIHidden = true; >;
float4x4 m_WVP : WorldViewProjection < bool UIHidden = true; >;
float4x4 m_WorldIT : WorldInverseTranspose < bool UIHidden = true; >;
float4x4 m_ViewInv : ViewInverse  < bool UIHidden = true; >;
float4x4 m_WorldI : WorldInverse  < bool UIHidden = true; >;

float4 g_cSSS <
    string UIWidget = "SSS Color";
> = {1.0f, 1.0f, 1.0f, 1.0f};     // Material's subsurface color
float4 g_cFresnel <
    string UIWidget = "Fresnel Color";
> = {1.0f, 1.0f, 1.0f, 1.0f};     // Material's specular color

float g_fOpacity : Opacity
<
	string UIWidget = "slider";
	string UIMin = "0.0";
	string UIMax = "1.0";
	string UIStep = "0.1";
	string UIName = "Transparency";
    string UIHelp = "The global transparency of the material";
> = 1.0f;
float g_fSpecMult
<
	string UIWidget = "slider";
	string UIMin = "0.0";
	string UIMax = "10.0";
	string UIStep = "0.05";
	string UIName = "Specular Multiplier";
    string UIHelp = "Increase or decrease the specular value";
> = 1.0f;
float g_fGlossiness : SPECULARPOWER
<
	string UIWidget = "slider";
	string UIMin = "2.0";
	string UIMax = "128.0";
	string UIStep = "1.0";
	string UIName = "Glossiness";
    string UIHelp = "Size of the specular highlight";
> = 1.0f;
float g_fAnisotropy
<
	string UIWidget = "slider";
	string UIMin = "0.0";
	string UIMax = "128.0";
	string UIStep = "0.1";
	string UIName = "Anisotropy";
    string UIHelp = "Anisotropy of the anisotropic specular highlight";
> = 1.0f;
float g_fFresnelPower
<
	string UIWidget = "slider";
	string UIMin = "0.0";
	string UIMax = "10.0";
	string UIStep = "0.1";
	string UIName = "Fresnel Power";
    string UIHelp = "Size of the Fresnel Effect";
> = 1.0f;
float g_fFresnelScale
<
	string UIWidget = "slider";
	string UIMin = "0.0";
	string UIMax = "10.0";
	string UIStep = "0.1";
	string UIName = "Fresnel Scale";
    string UIHelp = "Intensity of the Fresnel Effect";
> = 1.0f;
float g_fFresnelBias
<
	string UIWidget = "slider";
	string UIMin = "-5.0";
	string UIMax = "5.0";
	string UIStep = "0.1";
	string UIName = "Fresnel Bias";
    string UIHelp = "Bias the Fresnel Effect";
> = 0.0f; 
float g_fOffsetBias
<
	string UIWidget = "slider";
	string UIMin = "-1.0";
	string UIMax = "1.0";
	string UIStep = "0.01";
	string UIName = "Offset Height";
    string UIHelp = "Height of the Offset Mapping";
> = 0.0f;
float g_fAmbientIntensity
<
	string UIWidget = "slider";
	string UIMin = "0.0";
	string UIMax = "10.0";
	string UIStep = "0.1";
	string UIName = "Ambient Intensity";
    string UIHelp = "Intensity of the Ambient Lighting";
> = 1.0f;
float g_fMicroNormalScale
<
	string UIWidget = "slider";
	string UIMin = "0.0";
	string UIMax = "100.0";
	string UIStep = "0.01";
	string UIName = "Micro Normal Scale";
    string UIHelp = "Micro Normal Scale";
> = 1.0f;
int    g_nMinSamples <
    string UIWidget = "slider";
    int UIMin = 1;
    int UIMax = 200;
    int UIStep = 1;
    string UIName = "min samples";
> = 20;

int    g_nMaxSamples <
    string UIWidget = "slider";
    int UIMin = 2;
    int UIMax = 201;
    int UIStep = 1;
    string UIName = "max samples";
> = 60;

//Here you can turn on/off the lighting calculations
bool useBlinnSpec;
bool usePhongSpec;
bool useAnisotropicSpec;

bool useSingleBrdf;
bool useDualBrdf;
bool useSSS;

bool useFresnel;
bool useColorForFresnel;
bool useReflection;
bool useBlendedAmbient;
bool useSelfIllumination;

bool useOffsetMapping;
bool useParallaxOcclusion;
bool useMicroNormal;

float3 g_vLightPos : Position <
    string Object = "PointLight0";
    string UIName =  "Lamp 0 Position";
    string Space = "World";
> = {-0.5f,2.0f,1.25f};

////////////////////////////////////////////////////////
// Textures and Samplers
////////////////////////////////////////////////////////


////////////////////////////////////////////////////////
// Vertex Shader and Structs
////////////////////////////////////////////////////////

// properties read from each vertex
struct VS_INPUT
{
	float4 Position   : POSITION;  // untransformed vertex position
	float2 TexCoord  : TEXCOORD0; // vertex texture coordinates
	float3 Normal	: NORMAL0;
	float3 Binormal	: BINORMAL0;
	float3	Tangent : TANGENT0;
	float4 Color      : COLOR0;    // vertex color
	
};

// properties output for each vertex
struct VS_OUTPUT
{
	float4 Position  : POSITION;  // transformed vertex position
	float4 cVert   : COLOR0;    // vertex color
	float2 uvTexcoord  : TEXCOORD0; // final texture coordinates
	float3 vNormalWS : TEXCOORD1;
	float3 vBinormalWS : TEXCOORD2;
	float3 vTangentWS	: TEXCOORD3;
	float3 vLightWS	: TEXCOORD4;
	float3 vViewWS	: TEXCOORD5;
	float3 vViewTS	: TEXCOORD6;
  float2 vParallaxOffsetTS : TEXCOORD7;   // Parallax offset vector in tangent space
};

// Main pass vertex shader
void VS(in VS_INPUT In, out VS_OUTPUT Out)
{  
	Out.Position = mul(In.Position, m_WVP);
	Out.uvTexcoord = In.TexCoord;
	Out.cVert = In.Color;
	
	//World vectors
	Out.vNormalWS = mul(In.Normal, m_WorldIT);
	Out.vBinormalWS = mul(In.Binormal, m_WorldIT);
	Out.vTangentWS = mul(In.Tangent, m_WorldIT);
      
  	//world Position
  	float3 worldPosition = mul(In.Position, m_World);

	float3 vLightOS = g_vLightPos - worldPosition;
	Out.vLightWS = mul(vLightOS, m_World);
	
	Out.vViewWS = m_ViewInv[3] - worldPosition;
	
	//If we use offset mapping, we need the view vector in tangent space
	if (useOffsetMapping == true) {
		Out.vViewTS = Out.vViewWS;
	}
    
  if (useParallaxOcclusion == false) {
    Out.vParallaxOffsetTS = float2(0,0);
   }
   else {
        float3x3 mWorldToTangent = float3x3( Out.vTangentWS, Out.vBinormalWS, Out.vNormalWS );
        Out.vViewTS  = mul( mWorldToTangent, Out.vViewWS  );
        
        // Compute initial parallax displacement direction:
        float2 vParallaxDirection = normalize(  Out.vViewTS.xy );
           
        // The length of this vector determines the furthest amount of displacement:
        float fLength         = length( Out.vViewTS );
        float fParallaxLength = sqrt( fLength * fLength - Out.vViewTS.z * Out.vViewTS.z ) / Out.vViewTS.z; 
           
        // Compute the actual reverse parallax displacement vector:
        Out.vParallaxOffsetTS = vParallaxDirection * fParallaxLength;
           
        // Need to scale the amount of displacement to account for different height ranges
        // in height maps. This is controlled by an artist-editable parameter:
        Out.vParallaxOffsetTS *= g_fOffsetBias;
   }
}

////////////////////////////////////////////////////////
// Pixel Shader
////////////////////////////////////////////////////////

// Main pass pixel shader
float4 PS(in VS_OUTPUT In) : COLOR0
{
	//These are common vectors for whatever lighting function we use
	float3 V = normalize(In.vViewWS);
	float3 Nn = normalize(In.vNormalWS);
	float3 Bn = normalize(In.vBinormalWS);
	float3 Tn = normalize(In.vTangentWS);
	float3 L = normalize(In.vLightWS);
	float2 texCoord = In.uvTexcoord;
	
	//If we use offset mapping, we need to replace the existing UV coords
	if (useOffsetMapping == true) {
		float heightMap = tex2D(samplerNormal, In.uvTexcoord.xy).a;
		texCoord = OffsetMapping( V, heightMap, g_fOffsetBias, In.uvTexcoord);
	}
  if (useParallaxOcclusion == true) {
    texCoord = ParallaxOcclusionMapping (g_nMaxSamples, g_nMinSamples, V, Nn,
        In.vParallaxOffsetTS, In.uvTexcoord);
  }
	
	//Let's sample our maps
	float4 texDiffuse = tex2D(samplerDiffuse, texCoord);
	float4 texNormal = tex2D(samplerNormal, texCoord);
	float4 texSpecular = tex2D(samplerSpecular, texCoord);
  float4 texMask = tex2D(samplerMask, texCoord);
  //R = BRDF mask, G = reflectivity mas, B = SSS mask
  float fGlossiness = g_fGlossiness * texSpecular.a;
  
    //figure out N (normal)
    texNormal = NormalMapWSTransform (texNormal, Nn, Bn, Tn);  //Function returns unnormalized
    float3	N;
	 
    if (useMicroNormal == true) {
        float4 texMicroNormal = tex2D(samplerMicroNormal, texCoord * g_fMicroNormalScale);
        texMicroNormal = NormalMapWSTransform (texMicroNormal, Nn, Bn, Tn);
        N = normalize(texNormal.rgb + texMicroNormal.rgb);
    }
    else {
        N = normalize(texNormal.rgb);
    }
	//Opacity
	float opacity = texDiffuse.a * g_fOpacity;
	
	//Ambient, either a standard ambient cube or one lerped with a reflection mask
	float4 ambientColor;
	if (useBlendedAmbient == true) {
		float reflectionMask = texMask.g;
		ambientColor = CubeReflectionAmbient (N, texDiffuse, reflectionMask, g_fAmbientIntensity);
	}
	else {
		ambientColor = texCUBE(samplerAmbient, N);
		ambientColor *= g_fAmbientIntensity * texDiffuse;
	}
    //Glow?
    float4 cGlow = float4(0,0,0,1);
    if (useSelfIllumination == true) {
        cGlow = tex2D(samplerGlow, texCoord);
    }
	//to have reflection, either use reflection or use fresnel must be enabled
	float4 reflectionColor = float4(0,0,0,1);
    if ((useReflection == true) || (useFresnel == true)) {
        reflectionColor = texCUBE(samplerEnvironment, N);
        reflectionColor *= texMask.g;
    }
	
	//figure out our diffuse lighting
    //If we are using SSS, we need to bias the NDotL by the SSS mask
    float fNdotL = dot(N, L);
    if (useSSS == true) {
        float sssMask = texMask.b;
        fNdotL += sssMask;
    }
    
	float4 diffuseColor;
	if (useSingleBrdf == true) {
		diffuseColor = DiffuseSingleBRDF (V, N, texDiffuse, fNdotL);
	}
	else if (useDualBrdf == true) {
		float brdfMask = tex2D(samplerMask, texCoord).r;
		diffuseColor = DiffuseDualBRDF (V, N, texDiffuse, fNdotL, brdfMask);
	}
	else {
		fNdotL = saturate(fNdotL);
		diffuseColor = texDiffuse * fNdotL;
	}

	//Specular lighting
	float4 specularColor;
	if (useBlinnSpec == true) {
		specularColor = SpecularBlinn (V, L, N, texSpecular, fGlossiness, g_fSpecMult);
	}
	else if (usePhongSpec == true) {
		specularColor = SpecularPhong (V, L, N, texSpecular, fGlossiness, g_fSpecMult);
	}
	
	else if (useAnisotropicSpec == true) {
		float3 vecTangent = cross(N, Bn);
		specularColor = SpecularAnisotropic (V, L, vecTangent, texSpecular, g_fGlossiness, g_fSpecMult, g_fAnisotropy);
	}
	else specularColor = float4(0, 0, 0, 1); //no specular
	
	//Fresnel
    //we use a function to get the fresnel term
    //we then choose what to lerp between on the fresnel term with other booleans
	float4 cFresnel = float4(0,0,0,1);
	if (useFresnel == true) {
    float4 cGlance;
    float4 cStraight = float4(0,0,0,1);
    float fFresnel = FresnelEffect (V, N, g_fFresnelScale, g_fFresnelBias, g_fFresnelPower);
   
    if (useColorForFresnel == true) {
        cGlance = g_cFresnel;
    }
    else {
        cGlance = reflectionColor;
    }
    cFresnel = lerp(cGlance, cStraight, fFresnel);
	}
	
    //Put it all together
	float4 lightComp = ambientColor + diffuseColor + specularColor + cFresnel; 
    if (useReflection == true) { lightComp += reflectionColor; }
    //we need to add reflectance separately because otherwise it would be applied automatically if we have fresnel
    
    return float4(lightComp.xyz, opacity);
}

////////////////////////////////////////////////////////
// Techniques
////////////////////////////////////////////////////////

technique SimpleEffect
{
	pass Main
	{
		Vertexshader = compile vs_3_0 VS();
		alphablendenable = true;
    	srcblend = srcalpha;
    	destblend = invsrcalpha;
        CullMode = cw;
		PixelShader = compile ps_3_0 PS();
	}
}