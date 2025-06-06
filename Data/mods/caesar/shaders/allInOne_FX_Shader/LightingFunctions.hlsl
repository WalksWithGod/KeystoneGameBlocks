float4 SpecularBlinn(float3 vecView, float3 vecLight, float3 vecNormal, float4 texSpecular, float fGlossiness, float fSpecMult)
{
	float3 vecHalf = normalize(vecLight + vecView);
	float fNdotH = saturate(dot(vecNormal, vecHalf));
	float fSpecular = pow(fNdotH, fGlossiness);
	float4 cSpecular = texSpecular * fSpecular * fSpecMult;
	
	return cSpecular;
}

float4 SpecularPhong (float3 vecView, float3 vecLight, float3 vecNormal, float4 texSpecular, float fGlossiness, float fSpecMult)
{
	float3 vecReflection = normalize(2 * dot(vecView,vecNormal) * vecNormal - vecView);
	float fRdotL = saturate(dot(vecReflection, vecLight));
	float fSpecular = saturate(pow(fRdotL, fGlossiness));
	float4 cSpecular = texSpecular * fSpecular * fSpecMult;
	
	return cSpecular;
}

float4 SpecularAnisotropic (float3 vecView, float3 vecLight, float3 vecTangent, float4 texSpecular, float fGlossiness, float fSpecMult, float fAnisotropy)
{
	float3	vecHalf = normalize(vecLight + vecView);
    float	fSpecular = pow(sqrt(1 - (pow(dot(vecTangent, vecHalf), fAnisotropy))), fGlossiness);
	float4	cSpecular = texSpecular * fSpecular * fSpecMult;
    
    return cSpecular;
} 

float4 CubeReflectionAmbient (float3 vecNormal, float4 texDiffuse, float fReflectionMask, float fAmbientIntensity)
{
    float4	texReflection = texCUBE(samplerEnvironment, vecNormal);
    float4	texAmbient = texCUBE(samplerAmbient, vecNormal);
	float4	cAmbient = lerp(texAmbient, texReflection, fReflectionMask);
	cAmbient *= texDiffuse * fAmbientIntensity;
    
    return cAmbient;
}

float4 DiffuseSingleBRDF (float3 vecView, float3 vecNormal, float4 texDiffuse, float fNdotL)
{
	float	fNdotE = dot(vecNormal, vecView);
	float4	texBrdf = tex2D(samplerBRDF0, float2(fNdotL, 1 - fNdotE) * .5 + float2(.5,.5));
	return (texBrdf * texDiffuse);
} 

float4 DiffuseDualBRDF (float3 vecView, float3 vecNormal, float4 texDiffuse, float fNdotL, float fBrdfMask)
{
	float	fNdotE = dot(vecNormal, vecView);
	float4	texBrdf0 = tex2D(samplerBRDF0, float2(fNdotL, 1 - fNdotE) * .5 + float2(.5,.5));
	float4	texBrdf1 = tex2D(samplerBRDF1, float2(fNdotL, 1 - fNdotE) * .5 + float2(.5,.5));
	float4	cBrdf = lerp(texBrdf0, texBrdf1, fBrdfMask);

	return (cBrdf * texDiffuse);
}

float4 DiffuseSSS (float3 vecNormal, float3 vecLight, float4 texDiffuse, float4 cSSS, float fSSS)
{
    return 0;
}

float FresnelEffect (float3 vecView, float3 vecNormal, float fFresnelScale, float fFresnelBias, float fFresnelPower)
{
	float	fNdotE = saturate(dot(vecNormal, vecView));
	float	fFresnel = (pow(fNdotE, fFresnelPower) * fFresnelScale) + fFresnelBias;
	return fFresnel;
}

float2	OffsetMapping (float3 vecViewTS, float fHeightMap, float fOffsetBias, float2 uvOld)
{
	vecViewTS.y = -vecViewTS.y;
	float3 fOffset = vecViewTS * (fHeightMap * 2 - 1) * fOffsetBias;
	float2 uvNew = uvOld + fOffset;
	
	return uvNew;
}

float4	NormalMapWSTransform (float4 texNormal, float3 vecNormalWS, float3 vecBinormalWS, float3 vecTangentWS)
{
	texNormal = texNormal * 2 - 1;
	texNormal = float4((vecNormalWS * texNormal.z) + (texNormal.x * vecTangentWS + texNormal.y * -vecBinormalWS), 1.0);
	return texNormal;
}

float4	NormalMapWSTransform3ds (float4 texNormal, float3 vecNormalWS, float3 vecBinormalWS, float3 vecTangentWS)
{
	texNormal = texNormal * 2 - 1;
	//mnNose = float4((Nn * mnNose.z) + (mnNose.x * Bn + mnNose.y * -Tn), mnNose.w);
	texNormal = float4((vecNormalWS * texNormal.z) + (texNormal.x * vecBinormalWS + texNormal.y * -vecTangentWS), 1.0);
	return texNormal;
}

float2 ParallaxOcclusionMapping (float g_nMaxSamples, float g_nMinSamples, float3 vViewWS, float3 vNormalWS,
    float2 vParallaxOffsetTS, float2 texCoord)
{
  float2 dxSize, dySize;
   float2 dx, dy;

   float2 fTexCoordsPerSize = texCoord;
   float4( dxSize, dx ) = ddx( float4( fTexCoordsPerSize, texCoord ) );
   float4( dySize, dy ) = ddy( float4( fTexCoordsPerSize, texCoord ) );
   
  int nNumSteps = (int) lerp( g_nMaxSamples, g_nMinSamples, dot( vViewWS, vNormalWS ) );

  float fCurrHeight = 0.0;
  float fStepSize   = 1.0 / (float) nNumSteps;
  float fPrevHeight = 1.0;
  float fNextHeight = 0.0;

  int    nStepIndex = 0;
  bool   bCondition = true;

  float2 vTexOffsetPerStep = fStepSize * vParallaxOffsetTS;
  float2 vTexCurrentOffset = texCoord;
  float  fCurrentBound     = 1.0;
  float  fParallaxAmount   = 0.0;

  float2 pt1 = 0;
  float2 pt2 = 0;
   
  float2 texOffset2 = 0;

  while ( nStepIndex < nNumSteps ) 
  {
     vTexCurrentOffset -= vTexOffsetPerStep;

     // Sample height map which in this case is stored in the alpha channel of the normal map:
     fCurrHeight = tex2Dgrad( samplerNormal, vTexCurrentOffset, dx, dy ).a;

     fCurrentBound -= fStepSize;

     if ( fCurrHeight > fCurrentBound ) 
     {   
        pt1 = float2( fCurrentBound, fCurrHeight );
        pt2 = float2( fCurrentBound + fStepSize, fPrevHeight );

        texOffset2 = vTexCurrentOffset - vTexOffsetPerStep;

        nStepIndex = nNumSteps + 1;
        fPrevHeight = fCurrHeight;
     }
     else
     {
        nStepIndex++;
        fPrevHeight = fCurrHeight;
     }
  }   

  float fDelta2 = pt2.x - pt2.y;
  float fDelta1 = pt1.x - pt1.y;
  
  float fDenominator = fDelta2 - fDelta1;
  
  // SM 3.0 requires a check for divide by zero, since that operation will generate
  // an 'Inf' number instead of 0, as previous models (conveniently) did:
  if ( fDenominator == 0.0f )
  {
     fParallaxAmount = 0.0f;
  }
  else
  {
     fParallaxAmount = (pt1.x * fDelta2 - pt2.x * fDelta1 ) / fDenominator;
  }
  
  float2 vParallaxOffset = vParallaxOffsetTS * (1 - fParallaxAmount );

  // The computed texture offset for the displaced point on the pseudo-extruded surface:
  float2 texSampleBase = texCoord - vParallaxOffset;
  float2 texSample = texSampleBase;

    return texSample;
}