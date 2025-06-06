// Here is a texture parameter
texture Red
<
    string name = "./textures/rock.dds";
>;

texture NormalRed
<
    string name = "./textures/rock_normal.dds";
>;

texture Green
<
    string name = "./textures/grass.png";
>;

texture NormalGreen
<
    string name = "./textures/grass_normal.dds";
>;

texture Blue
<
    string name = "./textures/sand.dds";
>;

texture NormalBlue
<
    string name = "./textures/sand_normal.dds";
>;

texture Alpha
<
    string name = "./textures/snow.dds";
>;

texture NormalAlpha
<
    string name = "./textures/snow_normal.dds";
>;


float fTextureScale
<
   string UIName = "TextureScale";
   string UIWidget = "Numeric";
   bool UIVisible = true;
   float UIMin = 0.00;
   float UIMax = 1000000.00;
> = 5.f;

float4x4 matWorldViewProjection   : WORLDVIEWPROJECTION;
float4x4 matWorld                 : WORLD;
float4 vecCamera                  : VIEWPOS;

float4x4 matInvTransposeWorld     : WORLDTRANSPOSEINVERSE;

float4 vecLightDirDay                : LIGHTDIR0_DIRECTION;
float4 vDiffuseColorDay              : LIGHTDIR0_COLOR;

float4 vecLightDirNight;
float4 vDiffuseColorNight
<
    string UIName = "NightColor";
> = { 0.15f, 0.48f, 1.0f, 1.f };





sampler RedSampler = sampler_state
{ 
    Texture = (Red);
    MinFilter = LINEAR;
    MagFilter = LINEAR;
    MipFilter = LINEAR;
    AddressU = WRAP;
    AddressV = WRAP;    
};

sampler NormalRedSampler = sampler_state
{ 
    Texture = (NormalRed);
    MinFilter = LINEAR;
    MagFilter = LINEAR;
    MipFilter = LINEAR;
    AddressU = WRAP;
    AddressV = WRAP;    
};

sampler GreenSampler = sampler_state
{ 
    Texture = (Green);
    MinFilter = LINEAR;
    MagFilter = LINEAR;
    MipFilter = LINEAR;
    AddressU = WRAP;
    AddressV = WRAP;    
};

sampler NormalGreenSampler = sampler_state
{ 
    Texture = (NormalGreen);
    MinFilter = LINEAR;
    MagFilter = LINEAR;
    MipFilter = LINEAR;
    AddressU = WRAP;
    AddressV = WRAP;    
};

sampler BlueSampler = sampler_state
{ 
    Texture = (Blue);
    MinFilter = LINEAR;
    MagFilter = LINEAR;
    MipFilter = LINEAR;
    AddressU = WRAP;
    AddressV = WRAP;    
};

sampler NormalBlueSampler = sampler_state
{ 
    Texture = (NormalBlue);
    MinFilter = LINEAR;
    MagFilter = LINEAR;
    MipFilter = LINEAR;
    AddressU = WRAP;
    AddressV = WRAP;    
};

sampler AlphaSampler = sampler_state
{ 
    Texture = (Alpha);
    MinFilter = LINEAR;
    MagFilter = LINEAR;
    MipFilter = LINEAR;
    AddressU = WRAP;
    AddressV = WRAP;    
};

sampler NormalAlphaSampler = sampler_state
{ 
    Texture = (NormalAlpha);
    MinFilter = LINEAR;
    MagFilter = LINEAR;
    MipFilter = LINEAR;
    AddressU = WRAP;
    AddressV = WRAP;    
};



struct VS_OUTPUT
{
    float4 Pos : POSITION;
    float3 LightDay : TEXCOORD0;
    float3 LightNight : TEXCOORD1;
    float3 Normal : TEXCOORD2;
    float3 View : TEXCOORD3;
    float4 Channel : TEXCOORD4;
    float2 Tex1 : TEXCOORD5;
};



VS_OUTPUT VS(float4 Pos: POSITION, float2 Tex: TEXCOORD, float3 Normal : NORMAL, float3 Tangent : TANGENT)
{
    VS_OUTPUT Out = (VS_OUTPUT)0;

    //Transform the vector by the world view projection matrix
    Out.Pos = mul(Pos, matWorldViewProjection );

    //Return a tex coord to the pixel shader
    Out.Tex1 = Pos.xz / fTextureScale;
 
    //Red is rock
    //Green is grass
    //Alpha is snow
    //Blue is sand

    //Set the correct channel
    if ( Pos.y <= 55.f )
    {
       Out.Channel.a = 1.f;
       Out.Channel.rbg = 0.f;
    }
    if ( Pos.y <= 28.f )
    {
       Out.Channel.r = 1.f;
       Out.Channel.agb = 0.f;
    }
    if ( Pos.y  <= 21.f )        
    {
       Out.Channel.g  = 1.f;    
       Out.Channel.arb = 0.f;
    }
    if ( Pos.y  < 6.f )
    {
       Out.Channel.b = 1.f;    
       Out.Channel.agr = 0.f;
    }

    //Get a vectice of the current vertice multiplied by the world matrix
    float4 PosWorld = mul(Pos, matWorld );

    //Return the light vectors, this is superflous
    Out.LightDay = vecLightDirDay;
    Out.LightNight = vecLightDirNight;
 
    //Transform the normal
    Out.Normal = normalize(mul(matInvTransposeWorld, Normal));

    //return the camera position
    Out.View = normalize(vecCamera - PosWorld);

    //Return the values
    return Out;
}


float4 PS( VS_OUTPUT In ) : COLOR
{
    //Gets colors from textures
    float4 RedColor = tex2D(RedSampler, In.Tex1 );
    float4 GreenColor = tex2D(GreenSampler, In.Tex1 );
    float4 BlueColor = tex2D(BlueSampler, In.Tex1 );
    float4 AlphaColor = tex2D(AlphaSampler, In.Tex1 );

    //Multiplies this color by the Channel, calculated in the vertex shader
    RedColor *= In.Channel.r;
    GreenColor *= In.Channel.g;
    BlueColor *= In.Channel.b;
    AlphaColor *= In.Channel.a;

    //Normal Map
    float3 NormalRedColor = (( 2 * (tex2D(NormalRedSampler, In.Tex1 ) ) ) - 1.0f ) * In.Channel.r;
    float3 NormalBlueColor = (( 2 * (tex2D(NormalBlueSampler, In.Tex1 ) ) ) - 1.0f ) * In.Channel.b;
    float3 NormalGreenColor = (( 2 * (tex2D(NormalGreenSampler, In.Tex1 ) ) ) - 1.0f ) * In.Channel.g;
    float3 NormalAlphaColor = (( 2 * (tex2D(NormalAlphaSampler, In.Tex1 ) ) ) - 1.0f ) * In.Channel.a;

    //Normalize the vectors
    float3 LightDirDay = normalize(In.LightDay); //L
    float3 LightDirNight = normalize(In.LightNight);
    float3 Viewdir = normalize(In.View);   //V

    //Returns the final color
    float4 Color = AlphaColor + RedColor + GreenColor + BlueColor;
    
    //Setup the ambient lighting
    float  Aintensity = 0.05f;
    float4 Acolor = { 1.0f, 1.0f, 1.0f, 1.0 };
    float4 Alighting = ( Aintensity * Acolor );
    Alighting.a = 1.f;

    //Diffuse calculated from normal map
    float4 DiffuseDay = saturate(dot(In.Normal + NormalRedColor + NormalBlueColor + NormalGreenColor + NormalAlphaColor, LightDirDay ) ) * vDiffuseColorDay;
    float4 DiffuseNight = saturate(dot(In.Normal + NormalRedColor + NormalBlueColor + NormalGreenColor + NormalAlphaColor, LightDirNight ) ) * vDiffuseColorNight;

    //Calculate the reflection for specular lighting
    float3 ReflectDay = normalize(2 * DiffuseDay * (In.Normal) - LightDirDay );
    float3 ReflectNight = normalize(2 * DiffuseNight * (In.Normal) - LightDirNight );

    //Calculate the specular lighting
    float SpecularDay = pow(saturate(dot(ReflectDay ,Viewdir)),8);
    float SpecularNight = pow(saturate(dot(ReflectNight,Viewdir)),8);

    //Return the colour of the texel
    return Color * (Alighting + DiffuseNight + SpecularNight + DiffuseDay + SpecularDay );
}


technique tec0
{
    pass p0
    {
        //Standard compiling

        VertexShader = compile vs_1_1 VS();
        PixelShader = compile ps_2_0 PS();
    }
}