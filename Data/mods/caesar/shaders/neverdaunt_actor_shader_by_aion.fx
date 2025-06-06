// http://www.truevision3d.com/forums/showcase/neat_mesh_and_actor_shaders-t17704.0.html;wap2=
///////////////////////////////////////////////////
// Neverdaunt Custom Actor Shader 
// with smoothed diffuse and backglow
//
//   by Aion
//   do with as you please
///////////////////////////////////////////////////



// here is the non bumpmapped version, so there is no Tangent nor Binormal vector :
struct VS_INPUT_NONBUMP
{
 float3 position : POSITION ;    // vertex position in the default pose
    float3 normal : NORMAL;    // vertex normal in the default pose
    float4 blendindices : BLENDINDICES;  // indices of the 4 bone indices used in the bone matrix array
    float4 blendweights : BLENDWEIGHT; // weights of each bone.
    float2 tex1 : TEXCOORD0; // standard texture coordinates.
};

// here is the bumpmapped version, with tangents (i havent tested this, or even thought about it):
struct VS_INPUT_BUMP
{
 float3 position : POSITION;
 float3 normal : NORMAL; //
 float3 tangent : TANGENT; //  this forms the tangent matrix.
        float3 binormal : BINORMAL; //
 float4 blendindices : BLENDINDICES;
 float4 blendweights : BLENDWEIGHT;
 float2 tex1 : TEXCOORD0;
};

// a matrix here is only using 3 rows (float4x3) so we will use an array of float4x3, it MUST have the semantic BONES
float4x3 boneMatrices[52] : BONES;

// here is the number of bones used per vertex 
int iNumBonePerVertex : BONESPERVERTEX;  

// a simple world matrix is used when there is no bone influence (it must have the semantic WORLD);
float4x4 worldMat : WORLD;

// usual viewprojection to complete the transformation
float4x4 ViewProj : VIEWPROJECTION;

float3 dirLightDir; //: LIGHTDIR;  this is broken :\ so you'll have to set it from TV
float3 materialEmissive : EMISSIVE;
float3 materialAmbient : AMBIENT;
float4 materialDiffuse : DIFFUSE;
float3 materialSpecular : SPECULAR;
float materialPower : SPECULARPOWER;
float3 dirLightColor; // : LIGHTDIR0_COLOR; also broken, set it manualy
float3 viewdir;// : VIEWPOS; again.. borked.. you'll need to set it every frame :\

float glowpower = 3; //this changes how much backglow you have
float4 glowcolor = (1,1,1,1);//and its color

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



struct VS_OUTPUT
{
   float4 position : POSITION;
   float4 diffuse : COLOR0;
   float2 tex : TEXCOORD0;  
   float3 ldir : TEXCOORD1; 
   float3 normal : TEXCOORD2;
   float3 view : TEXCOORD3;
   float3 dlc : TEXCOORD4;
};

VS_OUTPUT ActorShader(uniform int iNumBones, VS_INPUT_NONBUMP inp)
{
   VS_OUTPUT o = (VS_OUTPUT)0;
   float3 pos, norm;
   
   // use our skinning method 
   TransPositionNormal( iNumBones, float4(inp.position, 1.0f), inp.normal, inp.blendweights, inp.blendindices, pos, norm);
   
   // then let's transform the position into clip space using view proj matrix
  float4 p = mul(float4(pos,1.0f), ViewProj);
   o.position = p;
   o.normal = norm;
   o.tex = inp.tex1;
   o.view = viewdir;
   o.dlc = dirLightColor;
   o.ldir = normalize(dirLightDir);
   o.diffuse = float4(normalize(dirLightDir), 1.0f);  // some weird const lighting computation
   return o;
}
 
texture texTexture : TEXTURE0;
sampler sampTexture = sampler_state {
 Texture = <texTexture>;
};

float4 ActorPixelShader(VS_OUTPUT inp) : COLOR0
{
   float3 light = normalize(-inp.ldir);
   float3 lcolor =  saturate(inp.dlc * (1, 1, 1));
   float3 view = normalize(inp.view);
   float3 inview = normalize(-view);
   float2 tcoor = inp.tex;
   float4 tColor = tex2D(sampTexture, tcoor);
   float3 ambient =  materialAmbient;
   float3 normish = normalize(inp.normal);
   float3 smooth = normalize(light + normish);
   float3 halfway = normalize(light + view);
   float3 specular = pow(saturate(dot(normish, halfway)), materialPower) * materialSpecular;
   float3 emissive = materialEmissive;

   float3 glowy = normalize(inview + normish);
   float3 glow = pow(saturate(dot(glowy / 1.2f, inview)), glowpower) * glowcolor.rgb;
   
 float3 diffuse = pow(saturate(dot(smooth , light)) * materialDiffuse.rgb, 1.5);
 
   float3 c = (saturate(ambient + diffuse ) * tColor.rgb + specular) * lcolor + emissive + glow;
  
   float alpha = materialDiffuse.a * tColor.a;
   return float4(c, alpha);
}

VertexShader VSArray[5] = { compile vs_2_0 ActorShader(0),
     compile vs_2_0 ActorShader(1),
     compile vs_2_0 ActorShader(2),
      compile vs_2_0 ActorShader(3),
     compile vs_2_0 ActorShader(4)
   };



technique ActorShader
{
    pass pass0
    {
       VertexShader = ( VSArray[iNumBonePerVertex] );
       PixelShader = compile ps_2_0 ActorPixelShader();
    }
}



Mesh Shader: neato.fx
Code:

///////////////////////////////////////////////////
// Neverdaunt Custom Mesh Shader 
// with smoothed diffuse and backglow
//
//   by Aion
//   do with as you please
///////////////////////////////////////////////////

// The combined world-view-projection matrix
float4x4 matWorldViewProj : WORLDVIEWPROJECTION;
float4x3 matWorldIT : WORLDINVERSETRANSPOSE; // this is for transforming the objectspace?
float4x4 matWorld : WORLD; //this is the world space matrix
float3 viewPosition : VIEWPOSITION; //is the cameras position


float2 scale = (1,1); //set this to tile your texture
float glowpower = 3; //this changes how much backglow you have
float4 glowcolor = (1,1,1,1);

float3 dirLightDir : LIGHTDIR0_DIRECTION; //get the light direction from TV
float3 materialEmissive : EMISSIVE;
float3 materialAmbient : AMBIENT;
float4 materialDiffuse : DIFFUSE;
float3 materialSpecular : SPECULAR;
float materialPower : SPECULARPOWER;
float3 dirLightColor : LIGHTDIR0_COLOR;

texture texTexture : TEXTURE0;
sampler sampTexture = sampler_state {
 Texture = <texTexture>;
 AddressU = Wrap;
 AddressV = Wrap;
 AddressW = Wrap;
};

// The vertex shader input structure
struct VS_INPUT {
 float4 position : POSITION;
 float3 normal : NORMAL;
 float2 texCoord : TEXCOORD0;
};

// vertex shader output
struct VS_OUTPUT {
  float4 position : POSITION;
  float2 texCoord : TEXCOORD0;
  float3 normal : TEXCOORD1;
  float3 view : TEXCOORD2;
  float3 lightout : TEXCOORD3;
};

// out of the Pixel shader and into the Vertex shader
#define PS_INPUT VS_OUTPUT
 
// Vertex shader function
VS_OUTPUT VS(VS_INPUT IN) {
 // Define an "instance" of the output structure
 VS_OUTPUT OUT;
 // Set the screen-space position
  float4 meh =  mul(IN.position, matWorldViewProj);
 
 OUT.position = meh;
 // And the texture coordinate
 OUT.texCoord = IN.texCoord;
 // Calculate the normal vector
// OUT.normal = mul(matWorldIT, IN.normal);
 //OUT.normal = normalize(mul(IN.normal, (float3x3)matWorld));
 //OUT.normal = mul( IN.normal, (float3x3)matWorldIT);

   OUT.normal = mul( IN.normal, matWorldIT);
 // Calculate the view vector
 float3 worldPos = mul(IN.position, matWorld).xyz;
  OUT.lightout = dirLightDir;
 OUT.view = viewPosition - worldPos;
 // Return this instance

 return OUT;
}
 
// Between the vertices and the pixels, the texture coordinates will be interpolated
 


// Pixel shader function
float4 PS(PS_INPUT IN) : COLOR {

  //normalize all vectors in the pixel shader
  float3 light = normalize(-IN.lightout);
  float3 view = normalize(IN.view);
  float3 inview = normalize(-view);
  float3 normal = normalize(IN.normal);
  // Calculate the half vectors
  float3 halfway = normalize(light + view);
  float3 glowy = normalize(inview + normal); 
  float3 smooth = normalize(light + normal);
  
  // Calculate the emissive lighting
 float3 emissive = materialEmissive;
 // Calculate the ambient reflection
 float3 ambient =  materialAmbient;
 // Calculate the diffuse reflection
 float3 diffuse = pow(saturate(dot(smooth , light)) * materialDiffuse.rgb, 1.5);
  float3 specular = pow(saturate(dot(normal, halfway)), materialPower) * materialSpecular;

  //the position of that pixel?
  float2 texCoord = IN.texCoord * scale;

    //texture color per pixel
    float4 texColor = tex2D(sampTexture, texCoord);

    float3 lightcolor = saturate(dirLightColor * (1, 1, 1)) ;
    //calc the back glow
    float3 glow = pow(saturate(dot(glowy / 1.2f, inview)), glowpower) * glowcolor.rgb;
 
 // Combine all the color components
 float3 color = (saturate(ambient + diffuse ) * texColor + specular ) * lightcolor + emissive + glow;
 
  // Calculate the transparency
 float alpha = materialDiffuse.a * texColor.a;
 // Return the pixel's color
 return float4(color, alpha);
}

technique TSM3 {
    pass P {
 VertexShader = compile vs_3_0 VS();
 PixelShader  = compile ps_3_0 PS();    
    }
}
technique TSM2a {
    pass P0 {  
VertexShader = compile vs_2_a VS();
 PixelShader  = compile ps_2_a PS(); 
    }
}
technique TSM2b {
    pass P0 {  
VertexShader = compile vs_2_0 VS();
 PixelShader  = compile ps_2_b PS(); 
    }
}
technique TSM2 {
    pass P {
 VertexShader = compile vs_2_0 VS();
 PixelShader  = compile ps_2_0 PS();    
    }
}
