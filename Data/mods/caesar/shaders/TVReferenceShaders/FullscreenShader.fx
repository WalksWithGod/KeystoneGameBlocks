/////////////////////////////////////////////
//
// TrueVision3d example for fullscreen shader.
// Used for scene shader and/or Draw_FullscreenQuadWithShader
// 
/////////////////////////////////////////////

// the texture declaration, you need samplers there.
// we will ask the engine to use the sampler 0 and 1 so we register them.
sampler texture1 : register(s0);
sampler texture2 : register(s1);



// Structure of the vertex.
// Basically you need only a float4 position, and one tex coord.

struct VS_INPUT
{
     float4 Position : POSITION;  // already transformed position.
     float2 TexCoord : TEXCOORD0; // texture coordinates passed via Draw_Custom or Draw_FullscrenQuadWithShader
};


// you can compute different things in the vertex shader
// and create several other texture coordinates as well.
// here i will show a way to have 2 texture coordinates for the pixel shader.
  
struct VS_OUTPUT
{
    float4 Position : POSITION; // just need to copy the position of the VS input in this one.
    float2 TexCoord1 : TEXCOORD0; // first set of tex coords.
    float2 TexCoord2 : TEXCOORD1; // second set of tex coords.
};

// vertex shader
VS_OUTPUT VS_Main(VS_INPUT input)
{
    // just allocate an output here.
    VS_OUTPUT output = (VS_OUTPUT)0;
    
    // copy the existing position into the output.
    output.Position = input.Position;
 
    // copy the first set of tex coord.
    output.TexCoord1 = input.TexCoord;
    
    // offset the second tex coord a little.
    output.TexCoord2 = input.TexCoord + float2(0.1f, 0.0f);

    // we're done.
    // we could also export the position into the Texcoord register to use it later on    
    // but it's a more advanced topic
    return output;
}

// pixel shader (interesting part of the effect)

float4 PS_Main(VS_OUTPUT input) : COLOR
{

   // ok let's get the two colors from textures.
   float4 tex1 = tex2D(texture1, input.TexCoord1);
   float4 tex2 = tex2D(texture2, input.TexCoord2);

   // just do an average of both, of course you could do more interesting things here.
   return lerp(tex1, tex2, 0.5f);
}


// now the definition of the technique.

technique FullscreenTech
{
   // we just use one pass here but you can of course add several passes with different shaders.
   pass FullscreenPass0
   {
       VertexShader = compile vs_1_1 VS_Main();
       PixelShader = compile ps_1_1 PS_Main();
   }
}

        
