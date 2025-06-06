///////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////
///BlindSided's Anisotropic Lighting Shader				///
///////////////////////////////////////////////////////////////////////////
///Free for public use, but please give credit/link =)                  ///
///								        ///
///BS, aka. Stephen Smithbower.  -> smithy.s@gmail.com                  ///
///			  	    http://smithbower.com/devblog       ///
///////////////////////////////////////////////////////////////////////////
///Features:								///
///									///
///-> Diffuse normal mapping						///
///-> Specular component modulated by alpha of diffuse texture		///
///   (perhapes I should change this?)					///
///////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////



///////////////////////////////////////////////////////////////////////////
/// SYMANTICS								///
///////////////////////////////////////////////////////////////////////////

float4x4 view_proj_matrix: VIEWPROJECTION;
float4x4 view_matrix: VIEW;

///////////////////////////////////////////////////////////////////////////
/// APPLICATION TWEAKABLES						///
///////////////////////////////////////////////////////////////////////////

float noiseRate;
float noiseScale;
float4 color;
float4 gloss;
float4 lightDir;


///////////////////////////////////////////////////////////////////////////
/// TEXTURES								///
///////////////////////////////////////////////////////////////////////////

texture NoiseMap;
sampler NoiseSampler = sampler_state {

    Texture = <NoiseMap>;

};


///////////////////////////////////////////////////////////////////////////
/// STRUCTS								///
///////////////////////////////////////////////////////////////////////////

struct vsOut {
   float4 Pos:      POSITION;
   float3 normal:   TEXCOORD0;
   float3 tangent:  TEXCOORD1;
   float3 binormal: TEXCOORD2;
   float3 viewVec:  TEXCOORD3;
   float3 pos:      TEXCOORD4;
};


///////////////////////////////////////////////////////////////////////////
/// VERTEX SHADER							///
///////////////////////////////////////////////////////////////////////////

void VS(in float3 pos: POSITION, float3 tangent: TANGENT, float3 binormal: BINORMAL, float3 normal: NORMAL, out vsOut Out) {

   Out.Pos = mul(pos, view_proj_matrix);

   Out.normal   =  mul(normal, view_matrix);
   Out.binormal =  mul(binormal, view_matrix);
   Out.tangent  =  mul(tangent, view_matrix);

   Out.viewVec  = -mul(pos, view_matrix);

   Out.pos = pos.xyz * noiseRate;
}


///////////////////////////////////////////////////////////////////////////
/// PIXEL SHADER							///
///////////////////////////////////////////////////////////////////////////

float4 PS(in float3 normal: TEXCOORD0, float3 tangent: TEXCOORD1, float3 binormal: TEXCOORD2, float3 viewVec: TEXCOORD3, float3 pos: TEXCOORD4) : COLOR {

   viewVec = normalize(viewVec);

   float angle = noiseScale * (tex3D(NoiseSampler, pos) - 0.5);

   float cosA, sinA;
   sincos(angle, sinA, cosA);

   float3 tang = sinA * tangent + cosA * binormal;

   float diffuse = saturate(dot(lightDir, normal));
   float cs = -dot(tang, viewVec);
   float sn = sqrt(1 - cs * cs);
   float cl = dot(tang, lightDir);
   float sl = sqrt(1 - cl * cl);
   float specular = pow(saturate(cs * cl + sn * sl), 32);

   return diffuse * color + gloss * specular;

}


///////////////////////////////////////////////////////////////////////////
/// TECHNIQUES								///
///////////////////////////////////////////////////////////////////////////

technique main
{
    pass p0 
    {		
		VertexShader = compile vs_1_1 VS();
		PixelShader  = compile ps_2_0 PS();
    }
}


