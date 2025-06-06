float4x4 matWorldViewProj : WORLDVIEWPROJECTION;
float4x3 matWorldIT : WORLDINVERSETRANSPOSE; //used for getting actual vector normal
float4x4 matWorld : WORLD; 
float3 viewPosition : VIEWPOSITION; 

// material lighting properties
float4 materialDiffuse : DIFFUSE; 
float3 materialEmissive : EMISSIVE;
float3 materialAmbient : AMBIENT; 
float3 materialSpecular : SPECULAR; 
float materialPower : SPECULARPOWER; 


float3 dirLightColor : LIGHTDIR0_COLOR; 
float3 dirLightDir : LIGHTDIR0_DIRECTION;

texture diffuseTexture : TEXTURE0;

sampler diffuseSampler = 
sampler_state
{
	Texture = (diffuseTexture);
};

struct VS_INPUT
{
	float4 position : POSITION;
	float3 normal : NORMAL;
	float2 texCoord : TEXCOORD0;

};

struct VS_OUTPUT
{
	float4 position : POSITION;
	float2 texCoord : TEXCOORD0;
	float3 normal : TEXCOORD1;
	float3 view : TEXCOORD2;
};


// outline pass 
float LineThickness = 0.03f;
float4 LineColor = float4(1.0f, 0.0f, 0.0f, 1.0f);

#define PS_INPUT VS_OUTPUT


// Vertex Shader
VS_OUTPUT VS(VS_INPUT IN)
{
	VS_OUTPUT OUT;

	OUT.position = mul(IN.position, matWorldViewProj);
	OUT.texCoord = IN.texCoord;

	//calc normal vec
	OUT.normal = mul(matWorldIT, IN.normal);

	//calc view vec
	float3 worldPos = mul(IN.position, matWorld).xyz;
	OUT.view = viewPosition - worldPos;

	return OUT;
}

// Pixel Shader
float4 PS(PS_INPUT IN) : COLOR
{
    float3 light = normalize(-dirLightDir);
	float3 view = normalize(IN.view);
	float3 normal = normalize(IN.normal);

	//Calc half vector average of the light and the view vectors
	float3 halfway = normalize(light+view);
	float3 emissive = materialEmissive;
	float3 ambient = materialAmbient;

	//calc diffuse reflection
	float3 diffuse = saturate(dot(normal,light)) * materialDiffuse.rgb;

	//calc specular reflection
	float3 specular = pow(saturate(dot(normal,halfway)),materialPower) * materialSpecular;

	float2 texCoord = IN.texCoord;
	float4 texColor = tex2D(diffuseSampler,texCoord);

	//combine all the color components
	//float3 color = (saturate(ambient + diffuse) * texColor + specular) * dirLightColor + emissive;

	float3 color = (saturate(ambient + diffuse) * texColor + specular) * dirLightColor + emissive;

	//calc transparency
	float alpha = materialDiffuse.a * texColor.a;

	//return the pixel color
	return float4(color, alpha);
}



VS_OUTPUT Outline_VS(VS_INPUT IN)
{
	VS_OUTPUT OUT = (VS_OUTPUT)0;
 
    // Calculate where the vertex ought to be.  
//    float4 original = mul(mul(mul(input.Position, World), View), Projection);
	float4 original = mul (IN.position, matWorldViewProj);
	
	//float4 original = IN.position + IN.position * 0.2f;
	//original = mul (original, matWorldViewProj);
	
    // Calculates the normal of the vertex like it ought to be.
//    float4 normal = mul(mul(mul(input.Normal, World), View), Projection);
	float4 normal = mul (normalize(IN.normal), matWorldViewProj);
	
    // Take the correct "original" location and translate the vertex a little
    // bit in the direction of the normal to draw a slightly expanded object.
    // Later, we will draw over most of this with the right color, except the expanded
    // part, which will leave the outline that we want.
    OUT.position = original ; //+ original * 0.02; // (mul(LineThickness, normal));
	OUT.position.xy += float2(0.06f, 0.06f);
	return OUT;
}

VS_OUTPUT OutlineNegative_VS (VS_INPUT IN)
{
	VS_OUTPUT OUT = (VS_OUTPUT)0;
	float4 original = mul (IN.position, matWorldViewProj);
	OUT.position = original ; 
	OUT.position.xy += float2(-0.06f, -0.06f);
	return OUT;
}

VS_OUTPUT OutlinePositiveNegative_VS (VS_INPUT IN)
{
	VS_OUTPUT OUT = (VS_OUTPUT)0;
	float4 original = mul (IN.position, matWorldViewProj);
	OUT.position = original ; 
	OUT.position.xy += float2(0.06f, -0.06f);
	return OUT;
}

VS_OUTPUT OutlineNegativePositive_VS (VS_INPUT IN)
{
	VS_OUTPUT OUT = (VS_OUTPUT)0;
	float4 original = mul (IN.position, matWorldViewProj);
	OUT.position = original ; 
	OUT.position.xy += float2(-0.06f, 0.06f);
	return OUT;
}


float4 Outline_PS (PS_INPUT IN) : COLOR0
{
	return LineColor;
}
 
technique forward
{
    pass P0
	{
        VertexShader = compile vs_3_0 Outline_VS();
        PixelShader  = compile ps_3_0 Outline_PS();
		CullMode = CW;
    }
	
	pass P1
	{
        VertexShader = compile vs_3_0 OutlineNegative_VS();
        PixelShader  = compile ps_3_0 Outline_PS();
		CullMode = CW;
    }
	
	pass P2
	{
        VertexShader = compile vs_3_0 OutlinePositiveNegative_VS();
        PixelShader  = compile ps_3_0 Outline_PS();
		CullMode = CW;
    }
	
	pass P3
	{
        VertexShader = compile vs_3_0 OutlineNegativePositive_VS();
        PixelShader  = compile ps_3_0 Outline_PS();
		CullMode = CW;
    }
	
	pass P2
	{
		VertexShader = compile vs_3_0 VS();
        PixelShader  = compile ps_3_0 PS();
		CullMode = CCW;
	}
	
}
