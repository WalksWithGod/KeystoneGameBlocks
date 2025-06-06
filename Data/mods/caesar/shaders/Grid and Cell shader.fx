Shader "Grid" 
{
	Properties 
	{
 	  _GridThickness ("Grid Thickness", Float) = 0.01
 	  _GridSpacingX ("Grid Spacing X", Float) = 1.0
 	  _GridSpacingY ("Grid Spacing Y", Float) = 1.0
 	  _GridOffsetX ("Grid Offset X", Float) = 0
 	  _GridOffsetY ("Grid Offset Y", Float) = 0
 	  _GridColour ("Grid Colour", Color) = (0.5, 1.0, 1.0, 1.0)
 	  _BaseColour ("Base Colour", Color) = (0.0, 0.0, 0.0, 0.0)
 	}
 
  
	SubShader 
	{
 
	  Tags { "Queue" = "Transparent" }
  
	  Pass 
	  {
 		ZWrite Off
 		Blend SrcAlpha OneMinusSrcAlpha
  
		CGPROGRAM
  
		// Define the vertex and fragment shader functions
 		#pragma vertex vert
 		#pragma fragment frag
  
 		// Access Shaderlab properties
 		uniform float _GridThickness;
 		uniform float _GridSpacingX;
 		uniform float _GridSpacingY;
 		uniform float _GridOffsetX;
 		uniform float _GridOffsetY;
 		uniform float4 _GridColour;
 		uniform float4 _BaseColour;
 
  
		// Input into the vertex shader
 
		struct vertexInput 
		{
 
			float4 vertex : POSITION;
 
		};
 
 
 
		// Output from vertex shader into fragment shader
 		struct vertexOutput 
		{
 		  float4 pos : SV_POSITION;
 		  float4 worldPos : TEXCOORD0;
 		};
 
 
 
		// VERTEX SHADER
 		vertexOutput vert(vertexInput input) 
		{
 		  vertexOutput output;
 
		  output.pos = mul(UNITY_MATRIX_MVP, input.vertex);
 
		  // Calculate the world position coordinates to pass to the fragment shader
 		  output.worldPos = mul(_Object2World, input.vertex);
 
		  return output;
 		}
 
 
 
		// FRAGMENT SHADER
 		float4 frag(vertexOutput input) : COLOR 
		{
			// frac() is like modulus but returns fractional remainder
			if (frac((input.worldPos.x + _GridOffsetX)/_GridSpacingX) < (_GridThickness / _GridSpacingX) ||
				frac((input.worldPos.y + _GridOffsetY)/_GridSpacingY) < (_GridThickness / _GridSpacingY)) 
			{
				return _GridColour;
			}
			else 
			{	
				return _BaseColour;
			}
 		}
 
	ENDCG
 	}
   }
 }
 ///////////////////////////////////
 http://code.google.com/p/wyverns-assault/source/browse/#svn%2Ftrunk%2Fdata%2Fmaterials%2Fshaders

float offset;

//The outline vertex shader
//This tranforms the model and
//"peaks" the surface (scales it out on it's normal)
VS_OUTPUT2 outline_vp(VS_INPUT Input)
{
	VS_OUTPUT2 Output;

	float3 transformedPosition = mul(Input.Position, WorldViewProjection);
	float3 transformedNormal = mul(Input.Normal, WorldViewProjection);
	
	Output.Normal = transformedNormal;
	Output.Position = transformedPosition + (mul(offset, transformedNormal));

	return Output;
}

// this is very basic type of outline shader. it only gives siloquette style outline
// and not an edge outline 
float4 black_fp() : COLOR0
{
	// by drawing scaled up version of model in pass0 as all black
	// the pass1 normal render at normal scale will be drawn over the top
	// 
	return float4(0.0f, 0.0f, 0.0f, 1.0f);
}
///////////////////////////////////////////////////////////////////////////////
TOON
////////////////////////////////////////////////////////////////////////////
// Vertex Shader Parameters
uniform float3 LightPosition; // object space
uniform float3 EyePosition; // object space
uniform float4 shininess;
uniform float4x4 WorldViewProj;

// Pixel Shader Parameters

uniform float4 ambient;
uniform float4 diffuse;
uniform float4 specular;
uniform float4 emissive;

uniform float4 ambientLightColour;
uniform float4 diffuseLightColour;
uniform float4 specularLightColour;

uniform sampler2D celShadingRamps;
uniform sampler2D textureColor;

//Input for the vertex Shader
//There are two vertex shaders both take the same input
struct VS_INPUT
{
	float4 Position : POSITION0;
	float3 Normal : NORMAL0;
	float2 Texcoord : TEXCOORD0;
};

//Output for the Cel vertex shader
struct VS_OUTPUT
{
	float4 Position : POSITION0;
	float2 Texcoord : TEXCOORD0;
	float Diffuse : TEXCOORD1;
	float Specular : TEXCOORD2;
	float Edge : TEXCOORD3;
};

/* Cel shading vertex program for single-pass rendering
In this program, we want to calculate the diffuse and specular
ramp components, and the edge factor (for doing simple outlining)
For the outlining to look good, we need a pretty well curved model.
*/
VS_OUTPUT toon_vp(VS_INPUT Input)
{
	VS_OUTPUT Output;

	// calculate output position
	Output.Position = mul(WorldViewProj, Input.Position);

	// calculate light vector
	float3 N = normalize(Input.Normal);
	float3 L = normalize(LightPosition - Input.Position.xyz);

	// Calculate diffuse component
	Output.Diffuse = max(dot(N, L) , 0);

	// Calculate specular component
	float3 E = normalize(EyePosition - Input.Position.xyz);
	float3 H = normalize(L + E);
	Output.Specular = pow(max(dot(N, H), 0), shininess);
	
	// Mask off specular if diffuse is 0
	if (Output.Diffuse == 0) Output.Specular = 0;

	// Edge detection, dot eye and normal vectors
	Output.Edge = max(dot(N, E), 0);

	// pass the main uvs straight through unchanged
	Output.Texcoord = Input.Texcoord;

	return Output;
}

//Input for the Cel Pixel shader
struct PS_INPUT
{
	float2 Texcoord : TEXCOORD0;
	float Diffuse : TEXCOORD1;
	float Specular : TEXCOORD2;
	float Edge : TEXCOORD3;
};

float4 toon_fp(PS_INPUT Input) : COLOR0
{
	float2 diffuseInUv = float2(Input.Diffuse,0);
	float2 specularInUv = float2(Input.Specular,1);
	float2 edgeInUv = float2(Input.Edge,2);

	// Step functions from textures
	Input.Diffuse = tex2D(celShadingRamps, diffuseInUv).x;
	Input.Specular = tex2D(celShadingRamps, specularInUv).x;
	Input.Edge = tex2D(celShadingRamps, edgeInUv).x;

	float4 color = Input.Edge * (
		(diffuse * Input.Diffuse * diffuseLightColour) * 0.9 +
		(tex2D(textureColor,Input.Texcoord) * 0.8) +
		(specular * Input.Specular * specularLightColour) * 0.8 +
		(ambient*ambientLightColour) +
		(emissive * 0.5)
	);

	color.a = tex2D(textureColor,Input.Texcoord).a;

	return color;
}

         
