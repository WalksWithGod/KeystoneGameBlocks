//////////////////////
// Volume line shader. Based on the CG Volume Lines sample from NVidia.


float4x4 mWVP : WORLDVIEWPROJECTION;
float4x4 mWV : WORLDVIEW;


texture lineTexture : TEXTURE0;


sampler textureSampler = sampler_state
{
	Texture = (lineTexture);
	MipFilter = LINEAR;
	MinFilter = LINEAR;
	MagFilter = LINEAR;
};


struct TInputVertex
{
	float4 PositionMS			: POSITION;		// Position of this vertex
	float4 OtherPositionMS		: NORMAL;		// Position of the other vertex at the other end of the line.
	float4 texOffset			: TEXCOORD0;	// Tex coord offset.
	float3 thickness			: TEXCOORD1;	// Thickness info.
};


struct TOutputVertex
{
	float4 PositionWS			: POSITION;
	float blend 				: COLOR0;
	float4 tex0 				: TEXCOORD0;
	float4 tex1 				: TEXCOORD1;
};


TOutputVertex VolumeLineVS( TInputVertex IN )
{
	TOutputVertex OUT = (TOutputVertex)0;

	// World*View transformation.
	float4 posStart = mul( IN.PositionMS, mWV );
	float4 posEnd = mul( IN.OtherPositionMS, mWV );
	
	// Unit vector between eye and center of the line.
	float3 middlePoint = normalize( (posStart.xyz + posEnd.xyz) / 2.0 );
	
	// Unit vector of the line direction.
	float3 lineOffset = posEnd.xyz - posStart.xyz;
	float3 lineDir = normalize( lineOffset );
	float lineLenSq = dot( lineOffset, lineOffset );
	
	// Dot product to compute texture coef
	float texCoef = abs( dot( lineDir, middlePoint ) );
	
	// Change texture coef depending on line length: y=(Sz/(l^2))(x-1)+1
	texCoef = max( ( (texCoef - 1) * (lineLenSq / IN.thickness.z ) ) + 1, 0 );
	
	posStart = mul( IN.PositionMS, mWVP );
	posEnd = mul( IN.OtherPositionMS, mWVP );
	
	// Project these points in screen space.
	float2 startPos2D = posStart.xy / posStart.w;
	float2 endPos2D = posEnd.xy / posEnd.w;
	
	// Calculate 2D line direction.
	float2 lineDir2D = normalize( startPos2D - endPos2D );
	
	// Shift vertices along 2D projected line
	posStart.xy += ((texCoef * IN.texOffset.x) * lineDir2D.xy);
	
	// Shift vertex for thickness perpendicular to line direction.
	lineDir2D *= IN.thickness.x;
	posStart.x += lineDir2D.y;
	posStart.y -= lineDir2D.x;
	
	OUT.PositionWS = posStart;
	
	// Compute tex coords depending on texCoef.
	float4 tex;
	tex.zw = float2(0,1);
	tex.y = min(15.0/16.f, texCoef);
	tex.x = modf(tex.y * 4.0, tex.y);
	OUT.blend = modf(tex.x * 4.0, tex.x);
	tex.xy = (tex.xy / 4.0) + (IN.texOffset).zw;
	tex.y = 1-tex.y;
	OUT.tex0 = tex;
	
	// Now get the second texture coord : increment.
	
	tex.y = min(texCoef + (1.0/16.f), 15.0/16.0 );
	tex.x = modf(tex.y * 4.0, tex.y);
	tex.x = floor(tex.x * 4.0);
	tex.xy = (tex.xy / 4) + (IN.texOffset).zw;
	tex.y = 1-tex.y;
	OUT.tex1 = tex;
	
	return OUT;
}


float4 VolumeLinePS( TOutputVertex IN ) : COLOR
{
	float4 blendFactor = IN.blend;
	float4 c0 = tex2D( textureSampler, IN.tex0 );
	float4 c1 = tex2D( textureSampler, IN.tex1 );
	return lerp( c0, c1, blendFactor );
}


technique primary
{
	pass pass0
	{
		CullMode = none;
		AlphaBlendEnable = true;
		SrcBlend = one;
		DestBlend = one;
		alpharef = 0.9;
		alphafunc = GreaterEqual;
		ZEnable = false;
		VertexShader = compile vs_2_0 VolumeLineVS();
		PixelShader = compile ps_2_0 VolumeLinePS();
	}
}


////////////////////


// float PiOver2 = 1.5707963267948966192313216916398;

// uniform extern float4 BaseColor  = float4(0.25,1,0,1);
// float Zoom = 0.75;
// uniform extern float Thickness = 0.05;

// struct VertexShaderInput
// {
    // float4 Position : POSITION0;
	// float2 Tex0 : TexCoord0;
// };

// struct VertexShaderOutput
// {
    // float4 Position : POSITION0;
	// float2 Tex0 : TexCoord0;
// };



// //////////////////////////////////////////////////////////////////////////////////////////////////////////

// // common vertex shader, simple passthrough
// VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
// {
    // VertexShaderOutput output;
	
	// output.Position = input.Position;
	// output.Tex0 = input.Tex0;

    // return output;
// }


// // simple pixel shader to render the texture
// float4 PixelShaderRender(VertexShaderOutput input) : COLOR0
// {	
	// // remap and zoom
	// float x = 2 * input.Tex0.x - 1;
	// float y = 2 * input.Tex0.y - 1;
	// x /= Zoom;
	// y /= Zoom;
	
	// // calculate implicit circle formula for x� + y� = 1;
	// float implicit =  x * x + y * y;
	
	// // Calculate our upper and lower bounds based on thickness.
	// float lower = 1 - Thickness;
	// float upper = 1;	
	
	// // remap [lower;upper] to [-1,1] and effectively clamp to [lower;upper] 	
	// implicit = 2 * (implicit - lower) / Thickness - 1;
	// implicit = clamp(implicit, -1, 1);
	
	// // distribute for smoothing/antialiassing using 1 - implicit�
	// float density = 1 - (implicit * implicit * implicit * implicit);
	
	// // UNUSED - this alternative fades out more gradually, but is more costly
	// // distribute for antialiassing using cos(implicit * 0.5 * Pi)
	// // float density = cos(implicit * PiOver2);
	
	// float4 color = BaseColor;
	// color.a *= density;
	
	// return color;
// }

// // basic technique to render the texture
// technique Render
// {
    // pass Pass1
    // {        
        // VertexShader = compile vs_2_0 VertexShaderFunction();
        // PixelShader = compile ps_2_0 PixelShaderRender();
    // }
// }

