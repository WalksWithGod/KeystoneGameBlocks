// http://tech-artists.org/forum/showthread.php?421-HLSL-Animated-texture-shader
// -------------------------------------------------------------
//    Semantics
// -------------------------------------------------------------
float4x4 WorldViewProj : WorldViewProjection < string UIWidget="None"; >;
//float Time             : Time                < string UIWidget="None"; >;

// -------------------------------------------------------------
//    Parameters
// -------------------------------------------------------------

// component editor
float TileCountX = 16.0f;
float TileCountZ = 16.0f;

// dimensions in pixels (TODO: these two vars are not used)
float AtlasTextureWidth = 128.0f;
float AtlasTextureHeight = 8.0f; //16.0f;

float TileScaleX = 0.75f; // valid values 0.0f - 1.0f, shrinks the icons down a tiny bit
float TileScaleZ = 0.75f; // valid values 0.0f - 1.0f, shrinks the icons down a tiny bit

// NOTE: AtlasTextureCountX is image count, not width in pixels! Images within atlas can be >1 pixel wide!
float AtlasTextureCountX <
	string UIName = "Atlas Tiles X (Multiple of 2)";
> = 8.0f;

// NOTE: AtlasTextureCountY is image count, not height in pixels! Images within atlas can be >1 pixel high!
float AtlasTextureCountY <
	string UIName = "Atlas Tiles Y (Multiple of 2)";
> = 1.0f;

texture AtlasTexture : TEXTURE0;
texture LookupTexture : TEXTURE1;

sampler2D AtlasLookupSampler = sampler_state {
	  Texture = <AtlasTexture>;
	  MinFilter = Point;    // when using solid colors for our tiles, point and no Mip, and clamp eliminates edge bleeding.
	  MagFilter = Point;
	  MipFilter = None;
	  AddressU = Clamp;
	  AddressV = Clamp;
};


sampler2D LookupSampler = sampler_state {
	  Texture = <LookupTexture>;
	  MinFilter = Point;
	  MagFilter = Point;
	  MipFilter = None;
	  AddressU = Clamp;
	  AddressV = Clamp;
};
	
// -------------------------------------------------------------
//    Stage Interop
// -------------------------------------------------------------

struct AppData
{
	float3 Position : POSITION;
	float4 UV       : TEXCOORD0;
};

struct VertData
{
	float4 Position : POSITION;  // screen space position
	float2 UV       : TEXCOORD0;
	float3 HPos     : TEXCOORD1; // model space vertex position
};
// -------------------------------------------------------------
//    Vertex Shader
// -------------------------------------------------------------

VertData mainVS( AppData IN )
{
	VertData OUT;
	OUT.Position = mul(float4(IN.Position.xyz, 1.0f), WorldViewProj);
	OUT.HPos = IN.Position.xyz; // 

	OUT.UV = IN.UV.xy; 
	//OUT.UV *= scale;
	return OUT;
}

// -------------------------------------------------------------
//    Pixel Shader
// -------------------------------------------------------------
float4 mainPS( VertData IN ) : COLOR
{
	// which tile within the destination quad mesh are we on?
	// we floor because we want all UV's that fall within the width & height of a single tile to result in same tileX and tileZ values
	float tileX = floor (IN.UV.x * TileCountX);
	float tileZ = floor (IN.UV.y * TileCountZ);
	
	float atlasImageUVWidth =  1.0f / AtlasTextureCountX; 
	float atlasImageUVHeight = 1.0f / AtlasTextureCountY;

	// half texel allows us to find center of tile
	float atlasTextureHalfTexelWidth = 0.5f / AtlasTextureCountX; 
	float atlasTextureHalfTexelHeight = 0.5f / AtlasTextureCountY;
	
	// from lookup texture, grab atlas index we are using for the tile at tileX, tileZ
	float4 indices = tex2D(LookupSampler, IN.UV).rgba;

	// skip if pixel is fully transparent
	clip(indices.a - 1.0f); // calling clip function can disable early z optimizations on many gpus
		
	// convert the component rg floats back into atlas indices by multiplying by 255.0 then calc the UV in the AtlasSampler from that
	// http://drilian.com/2008/11/25/understanding-half-pixel-and-half-texel-offsets/
	// http://msdn.microsoft.com/en-us/library/bb219690%28VS.85%29.aspx
	float wOffset = indices.x * 255.0f * atlasImageUVWidth; // + atlasTextureHalfTexelWidth;
	float hOffset = indices.y * 255.0f * atlasImageUVHeight; // + atlasTextureHalfTexelHeight; 
	
	// compute the 0.0 - 1.0 ratio of where the input UV which is currently mapping to a single
	// large quad, maps to the current tile
	float ratioX = (IN.UV.x * TileCountX) - tileX; 
	float ratioZ = (IN.UV.y * TileCountZ) - tileZ;
	
	float scaleX = wOffset + (atlasImageUVWidth * ratioX);
	float scaleZ = hOffset + (atlasImageUVHeight * ratioZ);

	IN.UV = float2(scaleX, scaleZ);
	
	return tex2D(AtlasLookupSampler, IN.UV);
}

technique technique0 {
	pass p0 {
		CullMode = None;
		VertexShader = compile vs_3_0 mainVS();
		PixelShader = compile ps_3_0 mainPS();
	}
}