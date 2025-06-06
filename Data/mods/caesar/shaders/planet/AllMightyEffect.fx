// the light origin is already in world space, so if you 
// multiply it by the world_view_projection, you'll be translating it twice
// with the world matrix in there.  Just use the view*projection matrix to get the light
// into the screen. 
//-

//= vertex and pixel structures

struct CrepRayModel_VertexToPixel
{
    float4 Position     : POSITION;   // screen position: Can't us in the pixel shader
    float2 TexCoords    : TEXCOORD0;  // texture coordinates
    float3 Normal       : TEXCOORD1;  // for lighting calcs
    float3 Position3D   : TEXCOORD2;  // for lighting calcs
    float3 Orig_pos 	: TEXCOORD3;  // I need this sometimes
    float3 Orig_normal  : TEXCOORD4;  // and sometimes I need this too....
	float4 ray_origin   : TEXCOORD5;  // needed for crepuscular ray effect
	float4 screen_pos   : TEXCOORD6;  // needed for crepuscular ray effect
} ;

struct PixelToFrame
{  
    float4 Color        : COLOR0;
} ;


//= standard world matrices & wVP
float4x4 world_matrix : WORLD;
float4x4 world_view_projection : WORLDVIEWPROJECTION;
float4x4 view_projection : WORLDVIEW;


//= Textures and samplers
Texture   context_texture : TEXTURE0;
Texture noise_texture : TEXTURE1;

sampler context = sampler_state
{
  Texture = <context_texture>;
   
  magfilter = LINEAR; 
  minfilter = LINEAR; 
  mipfilter = LINEAR; 
  
  AddressU  = mirror; 
  AddressV  = mirror;
} ;

sampler3D Noise = sampler_state
{
  texture = <noise_texture>;
  mipfilter = linear;
  magfilter = linear;
  minfilter = linear;
  AddressU  = Mirror; 
  AddressV  = Mirror; 
  AddressW  = Mirror;
} ;

Texture model_textures : TEXTURE0;

sampler2D model_textures_sampler = sampler_state 
{ 
  texture = <model_textures> ; 
  MinFilter = LINEAR;
  MagFilter = LINEAR;
  MipFilter = LINEAR;
  AddressU  = mirror; 
  AddressV  = mirror;
} ;

//- 

float ticks : TIME;
float4 Eray_origin;

CrepRayModel_VertexToPixel CrepRayModel_VertexShader( float4 inPos : POSITION0, float3 inNormal: NORMAL0, float2 inTexCoords : TEXCOORD0)
{
    // below is standard VS stuff
	/**/	CrepRayModel_VertexToPixel Output = (CrepRayModel_VertexToPixel)0;
	/**/	Output.Orig_pos   = inPos.xyz;
	/**/	Output.Position   = mul(inPos, world_view_projection);
	/**/	Output.Normal     = normalize(mul(inNormal, (float3x3) world_matrix));
	/**/	Output.Position3D = mul(inPos, world_matrix);
	/**/	Output.TexCoords  = inTexCoords;
	/**/	Output.Orig_normal = inNormal;
	// ====================================================
	
	// ray-specific stuff below:  
		// translate the ray origin into projection space: Its already in world space, so just use view*projection
		/**/	Output.ray_origin = mul(Eray_origin,view_projection); 
		/**/	//Output.ray_origin.xyz /= (Output.ray_origin.w+.000001); 
	// put the vertex position to the PS in projection space too
	Output.screen_pos = Output.Position; //  / (Output.Position.w + .000001); // +.00001 without consequence but avoids divide by zero.
		
    return Output;
} ;

// Skybox vertex shader.
void context_VS( float3 pos : POSITION0,
						out float4 SkyPos : POSITION0 , 
						out float3 SkyCoord : TEXCOORD0 ,
						out float3 world_position: TEXCOORD1 ,
						out float4 screenpos: TEXCOORD2 ,
						out float4 origin    : TEXCOORD3 )
{
    SkyPos =  mul(pos, world_view_projection) ;        
    world_position = mul(pos,world_matrix);
	
	
	origin = mul(Eray_origin,view_projection); // put origin in projection space
	//origin.xyz /= (origin.w+.0000001);  

	screenpos = SkyPos;
	
	//screenpos.xyz = SkyPos.xyz / (SkyPos.w+ .000001); // +.00001 without consequence but avoids divide by zero.
	
    SkyPos.z = SkyPos.w-0.00001f; // set depth to maximum: Bug in nvidia cards makes artifacts if you use W, use slightly less than W.

    SkyCoord = pos;
	
    SkyCoord.x = -SkyCoord.x; // flip image left to right, your inside out remember!
} ;

float4 context_PS(float3 SkyCoord: TEXCOORD0 , float3 world_position: TEXCOORD1 , float4 screenpos: TEXCOORD2 , float4 origin: TEXCOORD3  ) : COLOR
{
	 screenpos.xyz /= screenpos.w;
	 origin.xyz /= origin.w;
	 
     float3 direction = normalize(origin.xyz - screenpos.xyz);
	 
	  direction.z =0.023f; // don't want to change the 3rd dimension of the volume, but can make an interesting effect if you do
	 
	 float ray_strength = pow(tex3D(Noise,direction-ticks),1.5).r;
	 float4 the_color = texCUBE(context, SkyCoord);
	 
	 // here I just amplify what is there, you can put some color on ray and add it in, whatever...
	 the_color.rgb +=  the_color.rgb * 4*ray_strength;  
	 
	 // in this example the rays will wrap all the way around the world.  You don't see them because the skysphere is black
	 // on the backside so the ray's don't amplify anything.    You might want to point a ray at the original origin and
	 // calculate the dot product between the geometry being rendered at the pixel and that origin and use that to attenuate the
	 // rays as they move around the skysphere.  I've done that before & I works/looks ok.
	 	 
	 return the_color;
    
} ;

// standard pixel shader for just returning the skybox texture color..
float4 normal_ps(float3 SkyCoord: TEXCOORD0 , float3 world_position: TEXCOORD1 ) : COLOR
{   
	 float4 tc = texCUBE(context, SkyCoord);
 
     return tc;
   
} ;

// pixel shader for the square
float4 square(CrepRayModel_VertexToPixel PSIn ) : COLOR
{
	//float4 the_color = float4(0,0,.75,1); 
	 float4 the_color = tex2D(context, PSIn.TexCoords);
	 PSIn.ray_origin.xyz /= PSIn.ray_origin.w;
	 PSIn.screen_pos.xyz /= PSIn.screen_pos.w;
	 
    float3 direction = normalize(PSIn.ray_origin.xyz - PSIn.screen_pos.xyz);
	 
	 direction.z = 0.023;
	 
	 // powing is necessary to shape your rays.. 
	 float ray_strength = pow(tex3D(Noise,direction-ticks),1.5).r;
	 	 
	 // you can obviously do anything here.  Give the ray color, intensify it etc..  you could use the 
	 // normal to see if the object is facing the ray, or not, and occlude the ray and so forth. But, 
	 // if you do that and don't draw the ray over the dark parts of the model, the ray will still continue
	 // once it goes past the model (on the skysphere) (in other words, the skysphere will have no knowledge
	 // that the ray is occluded and it will still appear there)
	 the_color += ray_strength;
	 
     return the_color;
    
} ;

// pixel shader for the ship
float4 phong_ship (CrepRayModel_VertexToPixel PSIn) : COLOR
{
  float3 eye_vector = normalize (-PSIn.Position3D); // camera is at 000
  
  float3 light_vector = normalize(Eray_origin-PSIn.Position3D );
  
  float  dotlight = saturate( dot (light_vector,PSIn.Normal));
  
  float3 reflection_vector = reflect(-light_vector,PSIn.Normal);
  
  float  phong = saturate( dot (eye_vector,reflection_vector) );
  
  phong = 8 * pow(phong , 10);  

  float4 the_color = tex2D(model_textures_sampler, PSIn.TexCoords);
  
  //the_color *= max(phong, dotlight);
  
  // add in the crepuscular ray hack
 PSIn.ray_origin.xyz /= PSIn.ray_origin.w;
	 PSIn.screen_pos.xyz /= PSIn.screen_pos.w;
	  
  float3 direction = normalize(PSIn.ray_origin.xyz - PSIn.screen_pos.xyz);
	 
   direction.z = 0.023;
 
   float ray_strength = pow(tex3D(Noise,direction-ticks),1.5).r;

   the_color *= ray_strength;
   
   the_color += float4(.25,.25,1,1) * 2*pow(ray_strength,2); 
   // assumes a little blue light..  changing the strength of the ray as it goes over a model looks kinda like it interacts
   //  end of ray hack
   
   
   // put a little background reflection into it..
   reflection_vector = reflect(-eye_vector,PSIn.Normal);
   
   //the_color+= texCUBE(context,reflection_vector);
   
   
   
   //the_color.a = 1;
   return the_color;
}

// skybox technique
//technique master_techniques
//{
//    pass context
//    {
//         VertexShader = compile vs_2_0 context_VS();
//         PixelShader = compile ps_2_0  context_PS();
//    }
//}


// square
technique squarewithrays
{
    pass pass0
    {
         VertexShader = compile vs_2_0 CrepRayModel_VertexShader();
         PixelShader = compile ps_2_0  square();
    }
}


// phonged models
//technique phongrays
//{
//    pass pass0
//    {
//         VertexShader = compile vs_2_0 CrepRayModel_VertexShader();
//         PixelShader = compile ps_2_0  phong_ship();
//    }
//}
 

