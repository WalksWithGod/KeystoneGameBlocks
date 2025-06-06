/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/SDK/9.5/SDK/MEDIA/HLSL/Quad.fxh#1 $

Copyright NVIDIA Corporation 2004
TO THE MAXIMUM EXTENT PERMITTED BY APPLICABLE LAW, THIS SOFTWARE IS PROVIDED
*AS IS* AND NVIDIA AND ITS SUPPLIERS DISCLAIM ALL WARRANTIES, EITHER EXPRESS
OR IMPLIED, INCLUDING, BUT NOT LIMITED TO, IMPLIED WARRANTIES OF MERCHANTABILITY
AND FITNESS FOR A PARTICULAR PURPOSE.  IN NO EVENT SHALL NVIDIA OR ITS SUPPLIERS
BE LIABLE FOR ANY SPECIAL, INCIDENTAL, INDIRECT, OR CONSEQUENTIAL DAMAGES
WHATSOEVER (INCLUDING, WITHOUT LIMITATION, DAMAGES FOR LOSS OF BUSINESS PROFITS,
BUSINESS INTERRUPTION, LOSS OF BUSINESS INFORMATION, OR ANY OTHER PECUNIARY LOSS)
ARISING OUT OF THE USE OF OR INABILITY TO USE THIS SOFTWARE, EVEN IF NVIDIA HAS
BEEN ADVISED OF THE POSSIBILITY OF SUCH DAMAGES.

Comments:
    Header file with lots of useful macros, types, and functions for use
		with textures and render-to-texture-buffer effects in DirectX.

Example Macro Usages:
	Texture-declaration Macros:
		FILE_TEXTURE_2D(SurfTexture,SurfSampler,"myfile.dds") // simple 2D wrap texture
		FILE_TEXTURE_2D_MODAL(SpotTexture,SpotSampler,"myfile.dds",CLAMP) // user-defined addr mode
	RenderTarget Texture-declaration Macros:
		DECLARE_QUAD_TEX(ObjTexture,ObjSampler,"A8R8G8B8")
		DECLARE_QUAD_DEPTH_BUFFER(DepthTexture,"D24S8")
		DECLARE_SIZED_QUAD_TEX(GlowTexture,GlowSampler,"A8R8G8B8",0.5) // scaled versions of above
		DECLARE_SIZED_QUAD_DEPTH_BUFFER(DepthTexture,"D24S8",0.5)
		DECLARE_SIZED_TEX(BlahMap,BlahSampler,"R32F",128,1)	// address mode is "clamp"
		DECLARE_SQUARE_QUAD_TEX(ShadTexture,ShadObjSampler,"A16R16G16B16F",512) // for shadows etc
		DECLARE_SQUARE_QUAD_DEPTH_BUFFER(ShadDepth,"D24S8",512)
	Data types used in shaders:
		QUAD_REAL & QUAD_REAL# -- defaults to HALF but you can define QUAD_REAL float
			before #including "Quad.fxh" either by explicitly declaring all the QUAD_REAL types
			yourself, or #define-ing the symbol QUAD_FLOAT
	Flags (define before #including "Quad.fxh"):
		TWEAKABLE_TEXEL_OFFSET	// shows in paramter panel
		NO_TEXEL_OFFSET	// disables

Structure:
	QuadVertexOutput -- used by shaders, defines simple connection for "Draw=buffer" VS and PS

Shader Functions for "Draw=buffer" passes:
	QuadVertexOutput ScreenQuadVS():
		standard vertex shader for screen-aligned quads
	QUAD_REAL4 TexQuadPS(QuadVertexOutput IN,uniform sampler2D InputSampler)
		pass this pixel shader a sampler -- will draw it to the screen
	QUAD_REAL4 TexQuadBiasPS(QuadVertexOutput IN,uniform sampler2D InputSampler,QUAD_REAL TBias)
		Same as above, but uses tex2Dbias()

Utility Functions for Texture-Based Lookup Tables:
	QUAD_REAL scale_lookup(QUAD_REAL Value,const QUAD_REAL TableSize)
	QUAD_REAL2 scale_lookup(QUAD_REAL2 Value,const QUAD_REAL TableSize)
	QUAD_REAL3 scale_lookup(QUAD_REAL3 Value,const QUAD_REAL TableSize)
Other Utility Functions:
	QUAD_REAL4 premultiply(QUAD_REAL4 C)
	QUAD_REAL4 unpremultiply(QUAD_REAL4 C) // uses macro value NV_ALPHA_EPSILON

Global Variables:
	QUAD_REAL QuadTexOffset // reconciles difference between pixel and texel centers
	QUAD_REAL2 QuadScreenSize // VIEWPORTPIXELSIZE, contains dimensions of render window

******************************************************************************/

#ifndef _QUAD_FXH
#define _QUAD_FXH

// Numeric types we are likely to encounter....
//	Redefine these before including "Quad.fxh" if you want
//		to use a type other than "half" for these data or just
//		define the symbol QUAD_FLOAT to use "floats"
#ifndef QUAD_REAL
#ifdef QUAD_FLOAT
#define QUAD_REAL float
#define QUAD_REAL2 float2
#define QUAD_REAL3 float3
#define QUAD_REAL4 float4
#define QUAD_REAL3x3 float3x3
#define QUAD_REAL4x3 float4x3
#define QUAD_REAL3x4 float3x4
#define QUAD_REAL4x4 float4x4
#else /* ! QUAD_FLOAT */
#define QUAD_REAL half
#define QUAD_REAL2 half2
#define QUAD_REAL3 half3
#define QUAD_REAL4 half4
#define QUAD_REAL3x3 half3x3
#define QUAD_REAL4x3 half4x3
#define QUAD_REAL3x4 half3x4
#define QUAD_REAL4x4 half4x4
#endif /* ! QUAD_FLOAT */
#endif /* ! QUAD_REAL */

///////////////////////////////////////////////////////////////////////
/// Texture-Declaration Macros ////////////////////////////////////////
///////////////////////////////////////////////////////////////////////

//
// Modal 2D File Textures
//
// example usage: FILE_TEXTURE_2D_MODAL(GlowMap,GlowSampler,"myfile.dds",CLAMP)
//
#define FILE_TEXTURE_2D_MODAL(TexName,SampName,Filename,AddrMode) texture TexName < \
	string ResourceName = (Filename); \
    string ResourceType = "2D"; \
>; \
sampler SampName = sampler_state { \
    texture = <TexName>; \
    AddressU  = AddrMode; \
    AddressV = AddrMode; \
    MipFilter = LINEAR; \
    MinFilter = LINEAR; \
    MagFilter = LINEAR; \
};

//
// Simple 2D File Textures
//
// example usage: FILE_TEXTURE_2D(GlowMap,GlowSampler,"myfile.dds")
//
#define FILE_TEXTURE_2D(TextureName,SamplerName,Diskfile) FILE_TEXTURE_2D_MODAL(TextureName,SamplerName,(Diskfile),WRAP)

//
// Use this variation of DECLARE_QUAD_TEX() if you want a *scaled* render target
//
// example usage: DECLARE_SIZED_QUAD_TEX(GlowMap,GlowSampler,"A8R8G8B8",1.0)
#define DECLARE_SIZED_QUAD_TEX(TexName,SampName,PixFmt,Multiple) texture TexName   < \
    float2 ViewPortRatio = {Multiple,Multiple}; \
    int MipLevels = 1; \
    string Format = PixFmt ; \
	string UIWidget = "None"; \
>; \
sampler SampName = sampler_state { \
    texture = <TexName>; \
    AddressU  = CLAMP; \
    AddressV = CLAMP; \
    MipFilter = POINT; \
    MinFilter = LINEAR; \
    MagFilter = LINEAR; \
};

//
// Use this macro to easily declare typical color render targets
//
// example usage: DECLARE_QUAD_TEX(ObjMap,ObjSampler,"A8R8G8B8")
#define DECLARE_QUAD_TEX(TextureName,SamplerName,PixelFormat) DECLARE_SIZED_QUAD_TEX(TextureName,SamplerName,(PixelFormat),1.0)

//
// Use this macro to easily declare variable-sized depth render targets
//
// example usage: DECLARE_SIZED_QUAD_DEPTH_BUFFER(DepthMap,"D24S8",0.5)
#define DECLARE_SIZED_QUAD_DEPTH_BUFFER(TextureName,PixelFormat,Multiple) texture TextureName : RENDERDEPTHSTENCILTARGET < \
    float2 ViewPortRatio = {Multiple,Multiple}; \
    string Format = (PixelFormat); \
	string UIWidget = "None"; \
>; 

//
// Use this macro to easily declare typical depth render targets
//
// example usage: DECLARE_QUAD_DEPTH_BUFFER(DepthMap,"D24S8")
#define DECLARE_QUAD_DEPTH_BUFFER(TexName,PixFmt) DECLARE_SIZED_QUAD_DEPTH_BUFFER(TexName,PixFmt,1.0)

//
// declare exact-sized arbitrary texture
//
// example usage: DECLARE_SIZED_TEX(BlahMap,BlahSampler,"R32F",128,1)
#define DECLARE_SIZED_TEX(Tex,Samp,Fmt,Wd,Ht) texture Tex : RENDERCOLORTARGET < \
	float2 Dimensions = { Wd, Ht }; \
    string Format = Fmt ; \
	string UIWidget = "None"; \
	int miplevels=1;\
>; \
sampler Samp = sampler_state { \
    texture = <Tex>; \
    AddressU  = CLAMP; \
    AddressV = CLAMP; \
    MipFilter = NONE; \
    MinFilter = LINEAR; \
    MagFilter = LINEAR; \
};

//
// declare exact-sized square texture, as for shadow maps
//
// example usage: DECLARE_SQUARE_QUAD_TEX(ShadMap,ShadObjSampler,"A16R16G16B16F",512)
#define DECLARE_SQUARE_QUAD_TEX(TexName,SampName,PixFmt,Size) DECLARE_SIZED_TEX(TexName,SampName,(PixFmt),Size,Size)

//
// likewise for shadow depth targets
//
// example usage: DECLARE_SQUARE_QUAD_DEPTH_BUFFER(ShadDepth,"D24S8",512)
#define DECLARE_SQUARE_QUAD_DEPTH_BUFFER(TextureName,PixelFormat,Size) texture TextureName : RENDERDEPTHSTENCILTARGET < \
	float2 Dimensions = { Size, Size }; \
    string Format = (PixelFormat) ; \
	string UIWidget = "None"; \
>; 

////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////// Utility Functions ////////
////////////////////////////////////////////////////////////////////////////

//
// Scale inputs for use with texture-based lookup tables. A value ranging from zero to one needs
//   a slight scaling and offset to be sure to point at the centers of the first and last pixels
//	 of that lookup texture. Pass the integer size of the table in TableSize
// For now we'll assume that all tables are 1D, square, or cube-shaped -- all axes of equal size
//
// Cost of this operation for pixel shaders: two const-register
//   entries and a MAD (one cycle)
QUAD_REAL scale_lookup(QUAD_REAL Value,const QUAD_REAL TableSize)
{
	QUAD_REAL scale = ((TableSize - 1.0)/TableSize);
	QUAD_REAL shift = (0.5 / TableSize);
	return (scale*Value + shift);
}

QUAD_REAL2 scale_lookup(QUAD_REAL2 Value,const QUAD_REAL TableSize)
{
	QUAD_REAL scale = ((TableSize - 1.0)/TableSize);
	QUAD_REAL shift = (0.5 / TableSize);
	return (scale.xx*Value + shift.xx);
}

QUAD_REAL3 scale_lookup(QUAD_REAL3 Value,const QUAD_REAL TableSize)
{
	QUAD_REAL scale = ((TableSize - 1.0)/TableSize);
	QUAD_REAL shift = (0.5 / TableSize);
	return (scale.xxx*Value + shift.xxx);
}

// pre-multiply and un-pre-mutliply functions. The precision
//		of thse operatoions is often limited to 8-bit so don't
//		always count on them!
// The macro value of NV_ALPHA_EPSILON, if defined, is used to
//		avoid IEEE "NaN" values that may occur when erroneously
//		dividing by a zero alpha (thanks to Pete Warden @ Apple
//		Computer for the suggestion in GPU GEMS II)

// multiply color by alpha to turn an un-premultipied
//	pixel value into a premultiplied one
QUAD_REAL4 premultiply(QUAD_REAL4 C)
{
	return QUAD_REAL4((C.w*C.xyz),C.w);
}

#define NV_ALPHA_EPSILON 0.0001

// given a premultiplied pixel color, try to undo the premultiplication.
// beware of precision errors
QUAD_REAL4 unpremultiply(QUAD_REAL4 C)
{
#ifdef NV_ALPHA_EPSILON
	QUAD_REAL a = C.w + NV_ALPHA_EPSILON;
	return QUAD_REAL4((C.xyz / a),C.w);
#else /* ! NV_ALPHA_EPSILON */
	return QUAD_REAL4((C.xyz / C.w),C.w);
#endif /* ! NV_ALPHA_EPSILON */
}

/////////////////////////////////////////////////////////////////////////////////////
// Structure Declaration ////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////

struct QuadVertexOutput {
   	QUAD_REAL4 Position	: POSITION;
    QUAD_REAL2 UV		: TEXCOORD0;
};

/////////////////////////////////////////////////////////////////////////////////////
// Hidden tweakables declared by this .fxh file /////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////

#ifndef NO_TEXEL_OFFSET

#ifdef TWEAKABLE_TEXEL_OFFSET
		QUAD_REAL QuadTexOffset = 0.5;
#else /* !TWEAKABLE_TEXEL_OFFSET */
		QUAD_REAL QuadTexOffset < string UIWidget="None"; > = 0.5;
#endif /* !TWEAKABLE_TEXEL_OFFSET */

	QUAD_REAL2 QuadScreenSize : VIEWPORTPIXELSIZE < string UIWidget="None"; >;

#endif /* NO_TEXEL_OFFSET */

////////////////////////////////////////////////////////////
////////////////////////////////// vertex shaders //////////
////////////////////////////////////////////////////////////

QuadVertexOutput ScreenQuadVS(
		QUAD_REAL3 Position : POSITION, 
		QUAD_REAL3 TexCoord : TEXCOORD0
) {
    QuadVertexOutput OUT;
    OUT.Position = QUAD_REAL4(Position, 1);
#ifdef NO_TEXEL_OFFSET
    OUT.UV = TexCoord.xy;
#else /* NO_TEXEL_OFFSET */
	QUAD_REAL2 off = QUAD_REAL2(QuadTexOffset/(QuadScreenSize.x),QuadTexOffset/(QuadScreenSize.y));
    OUT.UV = QUAD_REAL2(TexCoord.xy+off); 
#endif /* NO_TEXEL_OFFSET */
    return OUT;
}

//////////////////////////////////////////////////////
////////////////////////////////// pixel shaders /////
//////////////////////////////////////////////////////

// add glow on top of model

QUAD_REAL4 TexQuadPS(QuadVertexOutput IN,uniform sampler2D InputSampler) : COLOR
{   
	QUAD_REAL4 texCol = tex2D(InputSampler, IN.UV);
	return texCol;
}  

QUAD_REAL4 TexQuadBiasPS(QuadVertexOutput IN,uniform sampler2D InputSampler,QUAD_REAL TBias) : COLOR
{   
	QUAD_REAL4 texCol = tex2Dbias(InputSampler, QUAD_REAL4(IN.UV,0,TBias));
	return texCol;
}  

//////////////////////////////////////////////////////////////////
/// Macros to define passes within Techniques ////////////////////
//////////////////////////////////////////////////////////////////

// older HLSL syntax
#define TEX_TECH(TechName,SamplerName) technique TechName { \
    pass TexturePass  { \
		VertexShader = compile vs_2_0 ScreenQuadVS(); \
		AlphaBlendEnable = false; ZEnable = false; \
		PixelShader  = compile ps_2_a TexQuadPS(SamplerName); } }

#define TEX_BLEND_TECH(TechName,SamplerName) technique TechName { \
    pass TexturePass { \
		VertexShader = compile vs_2_0 ScreenQuadVS(); \
		ZEnable = false; AlphaBlendEnable = true; \
		SrcBlend = SrcAlpha; DestBlend = InvSrcAlpha; \
		PixelShader  = compile ps_2_a TexQuadPS(SamplerName); } }

// newer HLSL syntax

#define TEX_TECH2(TechName,SamplerName,TargName) technique TechName { \
    pass TexturePass  < \
		string ScriptFunction = "RenderColorTarget0=" (TargName) ";" \
    							"DrawInternal=Buffer;"; \
	> { \
		VertexShader = compile vs_2_0 ScreenQuadVS(); \
		AlphaBlendEnable = false; ZEnable = false; \
		PixelShader  = compile ps_2_a TexQuadPS(SamplerName); } }

#define TEX_BLEND_TECH2(TechName,SamplerName) technique TechName { \
    pass TexturePass < \
		string ScriptFunction = "RenderColorTarget0=" (TargName) ";" \
    							"DrawInternal=Buffer;"; \
	> { \
		VertexShader = compile vs_2_0 ScreenQuadVS(); \
		ZEnable = false; AlphaBlendEnable = true; \
		SrcBlend = SrcAlpha; DestBlend = InvSrcAlpha; \
		PixelShader  = compile ps_2_a TexQuadPS(SamplerName); } }

#endif /* _QUAD_FXH */

////////////// eof ///
