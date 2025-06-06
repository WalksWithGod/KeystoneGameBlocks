//Alpha holds opacity
texture DiffuseTexture : DiffuseMaterialTexture
<
  string ResourceName = "/Textures/Samples/DiginiTestCard.vtf";
  string UIName = "Diffuse Texture";
  string UIHelp = "Diffuse Surface Texture";
  string ResourceType = "2D";
>;

sampler2D samplerDiffuse = sampler_state
{
	Texture = (DiffuseTexture);
	AddressU = Wrap;
	AddressV = Wrap;
	AddressW = Clamp;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;	
};

//Alpha is height
texture NormalTexture : NormalMaterialTexture
<
  string ResourceName = "/Textures/Samples/DiginiTestCard.vtf";
  string UIName = "Normal Texture";
  string UIHelp = "Normal Surface Texture";
  string ResourceType = "2D";
>;

sampler2D samplerNormal = sampler_state
{
	Texture = (NormalTexture);
	AddressU = Wrap;
	AddressV = Wrap;
	AddressW = Clamp;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;	
};

//Alpha is glossiness factor
texture SpecularTexture : SpecularMaterialTexture
<
  string ResourceName = "/Textures/Samples/DiginiTestCard.vtf";
  string UIName = "Specular Texture";
  string UIHelp = "Specular Surface Texture";
  string ResourceType = "2D";
>;

sampler2D samplerSpecular = sampler_state
{
	Texture = (SpecularTexture);
	AddressU = Wrap;
	AddressV = Wrap;
	AddressW = Clamp;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;	
};

texture EnvironmentTexture : ENVIRONMENT <
	string ResourceName = "/Textures/Samples/DiginiTestCard.vtf";
	string UIName = "Environment Texture";
	string UIHelp = "Environment Surface Texture";
    string ResourceType = "Cube";
>;

samplerCUBE samplerEnvironment  = sampler_state {
    Texture = <EnvironmentTexture>;
	AddressU = Wrap;
	AddressV = Wrap;
	AddressW = Clamp;
    AddressU = Clamp;
    AddressV = Clamp;
    AddressW = Clamp;
};

texture AmbientTexture : AMBIENT <
	string ResourceName = "/Textures/Samples/DiginiTestCard.vtf";
	string UIName = "AmbientT Texture";
	string UIHelp = "AmbientT Surface Texture";
    string ResourceType = "Cube";
>;

samplerCUBE samplerAmbient  = sampler_state {
    Texture = <AmbientTexture>;
	AddressU = Wrap;
	AddressV = Wrap;
	AddressW = Clamp;
    AddressU = Clamp;
    AddressV = Clamp;
    AddressW = Clamp;
};

texture BRDF0Texture : BRDF0Texture
<
  string ResourceName = "/Textures/Samples/DiginiTestCard.vtf";
  string UIName = "BRDF0 Texture";
  string UIHelp = "BRDF0 Surface Texture";
  string ResourceType = "2D";
>;

sampler2D samplerBRDF0 = sampler_state
{
	Texture = (BRDF0Texture);
	AddressU = Clamp;
	AddressV = Clamp;
	AddressW = Clamp;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;	
};

texture BRDF1Texture : BRDF1Texture
<
  string ResourceName = "/Textures/Samples/DiginiTestCard.vtf";
  string UIName = "BRDF1 Texture";
  string UIHelp = "BRDF1 Surface Texture";
  string ResourceType = "2D";
>;

sampler2D samplerBRDF1 = sampler_state
{
	Texture = (BRDF1Texture);
	AddressU = Clamp;
	AddressV = Clamp;
	AddressW = Clamp;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;	
};

texture GlowTexture : GlowTexture
<
  string ResourceName = "/Textures/Samples/DiginiTestCard.vtf";
  string UIName = "Glow Texture";
  string UIHelp = "Glow Surface Texture";
  string ResourceType = "2D";
>;

sampler2D samplerGlow = sampler_state
{
	Texture = (GlowTexture);
	AddressU = Wrap;
	AddressV = Wrap;
	AddressW = Clamp;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;	
};

//R is BRDF Mask
//G is Reflectivity Mask
//B is SSS Mask
texture MaskTexture : MaskTexture
<
  string ResourceName = "/Textures/Samples/DiginiTestCard.vtf";
  string UIName = "Mask Texture";
  string UIHelp = "Mask Surface Texture";
  string ResourceType = "2D";
>;

sampler2D samplerMask = sampler_state
{
	Texture = (MaskTexture);
	AddressU = Wrap;
	AddressV = Wrap;
	AddressW = Clamp;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;	
};

texture MicroNormalTexture : MicroNormalTexture
<
  string ResourceName = "/Textures/Samples/DiginiTestCard.vtf";
  string UIName = "MicroNormal Texture";
  string UIHelp = "MicroNormal Surface Texture";
  string ResourceType = "2D";
>;

sampler2D samplerMicroNormal = sampler_state
{
	Texture = (MicroNormalTexture);
	AddressU = Wrap;
	AddressV = Wrap;
	AddressW = Clamp;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;	
};