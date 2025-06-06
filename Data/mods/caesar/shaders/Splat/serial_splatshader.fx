float4x4 matWorldViewProj : WORLDVIEWPROJECTION;

float L1Scale = 1;
float L2Scale = 1;
float L3Scale = 1;
float L4Scale = 1;


texture alpha : TEXTURE0;
texture layer1 : TEXTURE1;
texture layer2 : TEXTURE2;
texture layer3 : TEXTURE3;
texture layer4 : TEXTURE4;


sampler2D alphaSamp = sampler_state {
	Texture = (alpha);
	AddressU = Wrap;
	AddressV = Wrap;
};

sampler2D layerOne = sampler_state {
	Texture = (layer1);
	AddressU = Wrap;
	AddressV = Wrap;
};

sampler2D layerTwo = sampler_state {
	Texture = (layer2);
	AddressU = Wrap;
	AddressV = Wrap;
};

sampler2D layerThree = sampler_state {
	Texture = (layer3);
	AddressU = Wrap;
	AddressV = Wrap;
};

sampler2D layerFour = sampler_state {
	Texture = (layer4);
	AddressU = Wrap;
	AddressV = Wrap;
};


struct VS_INPUT {
	float4 position : POSITION;
	float2 AlphaTC : TEXCOORD0;
};


#define VS_OUTPUT VS_INPUT
#define PS_INPUT  VS_INPUT
 
VS_OUTPUT VS(VS_INPUT IN) {
	VS_OUTPUT OUT;
	OUT.position = mul(IN.position, matWorldViewProj);
	OUT.AlphaTC = IN.AlphaTC;
	return OUT;
}

float4 PS(PS_INPUT IN) : COLOR {
	
         
	float4 l1 = tex2D(alphaSamp, IN.AlphaTC).r * tex2D(layerOne, IN.AlphaTC * L1Scale); 
	float4 l2 = tex2D(alphaSamp, IN.AlphaTC).g * tex2D(layerTwo, IN.AlphaTC * L2Scale); 
	float4 l3 = tex2D(alphaSamp, IN.AlphaTC).b * tex2D(layerThree, IN.AlphaTC * L3Scale); 
	float4 l4 = tex2D(alphaSamp, IN.AlphaTC).a * tex2D(layerThree, IN.AlphaTC * L4Scale); 
	float4 final = l1 + l2 + l3 + l4;
	return final;

}


technique TSM1 {
    pass P0 {
        VertexShader = compile vs_2_0 VS();
        PixelShader  = compile ps_2_0 PS();
    }
}
