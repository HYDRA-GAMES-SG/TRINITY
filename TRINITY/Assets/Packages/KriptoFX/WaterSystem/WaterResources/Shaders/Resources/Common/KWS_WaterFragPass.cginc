half4 fragWater(v2fWater i) : SV_Target
{
	UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
	
	float2 screenUV = i.screenPos.xy / i.screenPos.w;
	float3 viewDir = GetWorldSpaceViewDirNorm(i.worldPosRefracted);
	float surfaceDepthZ = i.screenPos.z / i.screenPos.w;
	float surfaceDepthZEye = LinearEyeDepthUniversal(surfaceDepthZ);
	float sceneZ = GetSceneDepth(screenUV);
	float sceneZEye = LinearEyeDepthUniversal(sceneZ);
	half surfaceMask = KWS_UseWireframeMode == 1 || i.surfaceMask.x > 0.9999;
	half exposure = GetExposure();
	
	//float tile = GetReflectionProbeID(screenUV);
	//return float4(tile, 0, 0, 1);

	#ifdef KWS_VOLUME_MASK
		//float3 volumeMask = GetWaterVolumeMask(screenUV);
		//if(IsOutsideIntersectionVolumeMaskArea(volumeMask, i.screenPos.z / i.screenPos.w)) return 0;
		
		//float sdfDistance = GetWaterVolumeMaskProcedural(i.worldPosRefracted);
		//if(sdfDistance > 0.0) discard;

		//float waterMask = GetWaterMask(screenUV);
		//if(waterMask.x < 0.7 || waterMask > 0.8) discard;

		//float2 volumeData = GetWaterVolumePrePassVertex(i.worldPosRefracted);
		//if(volumeData.x > 0.01) discard;
		
	#endif
	/////////////////////////////////////////////////////////////  NORMAL  ////////////////////////////////////////////////////////////////////////////////////////////////////////

	float3 wavesNormalFoam;
	#if defined(KW_FLOW_MAP) || defined(KW_FLOW_MAP_EDIT_MODE) || defined(KW_FLOW_MAP_FLUIDS)
		wavesNormalFoam = GetFftWavesNormalFoamWithFlowmap(i.worldPos);
	#else
		wavesNormalFoam = GetFftWavesNormalFoam(i.worldPos);
	#endif
	float foam = wavesNormalFoam.y * surfaceMask;
	float3 tangentNormal = float3(wavesNormalFoam.x, 1, wavesNormalFoam.z);
	
	//return float4(foam.xxx, 1);

	
	#if defined(KW_FLOW_MAP_FLUIDS)
		half fluidsFoam;
		tangentNormal = GetFluidsNormal(i.worldPos, i.worldPos.xz / KW_FFTDomainSize, tangentNormal, fluidsFoam);
		fluidsFoam *= surfaceMask;
	#endif


	#ifdef KW_FLOW_MAP_EDIT_MODE
		return GetFlowmapEditor(i.worldPos, tangentNormal);
	#endif

	#if USE_SHORELINE
		tangentNormal = ComputeShorelineNormal(tangentNormal, i.worldPos);
		//return float4(tangentNormal.xz, 0, 1);
	#endif

	
	#if KW_DYNAMIC_WAVES
		float3 dynamicWavesNormal = GetDynamicWavesNormals(i.worldPos);
		tangentNormal = KWS_BlendNormals(tangentNormal, dynamicWavesNormal);
	#endif

	tangentNormal = lerp(float3(0, 1, 0), tangentNormal, surfaceMask);
	float3 worldNormal = KWS_BlendNormals(tangentNormal, i.worldNormal);

	/////////////////////////////////////////////////////////////  end normal  ////////////////////////////////////////////////////////////////////////////////////////////////////////
	//return float4(tangentNormal.xz, 0, 1);
	

	/////////////////////////////////////////////////////////////////////  REFRACTION  ///////////////////////////////////////////////////////////////////
	float2 refractionUV;
	half3 refraction;

	UNITY_BRANCH
	if (KWS_UseRefractionIOR > 0 && surfaceMask > 0.5) refractionUV = GetRefractedUV_IOR(viewDir, worldNormal, i.worldPosRefracted.xyz, sceneZEye, surfaceDepthZEye, KW_Transparent);
	else refractionUV = lerp(screenUV, GetRefractedUV_Simple(screenUV, worldNormal), surfaceMask);

	float refractedSceneZ = GetSceneDepth(refractionUV);
	float refractedSceneZEye = LinearEyeDepthUniversal(refractedSceneZ);
	FixRefractionSurfaceLeaking(surfaceDepthZEye, sceneZ, sceneZEye, screenUV, refractedSceneZ, refractedSceneZEye, refractionUV);
	
	UNITY_BRANCH
	if(KWS_UseRefractionDispersion > 0 && surfaceMask > 0.5)	refraction = GetSceneColorWithDispersion(refractionUV, KWS_RefractionDispersionStrength);
	else refraction = GetSceneColor(refractionUV);

	//return float4(refraction, 1);
	/////////////////////////////////////////////////////////////  end refraction  ////////////////////////////////////////////////////////////////////////////////////////////////////////
	


	
	/////////////////////////////////////////////////////////////////////  UNDERWATER  ///////////////////////////////////////////////////////////////////
	
	
	float2 volumeDepth = GetWaterVolumeDepth(screenUV, surfaceDepthZ, sceneZ, 0);
	half4 volumeLight = GetVolumetricLightWithAbsorbtion(screenUV, refractionUV, KW_Transparent, KW_TurbidityColor, KWS_DyeColor, refraction, volumeDepth, KWS_WaterInstanceID, exposure, 0);
	if(surfaceMask < 0.5) return float4(volumeLight.xyz, 1);
	
	float depthAngleFix = (surfaceMask < 0.5 || KWS_MeshType == KWS_MESH_TYPE_CUSTOM_MESH) ?        0.25 : saturate(GetWorldSpaceViewDirNorm(i.worldPos - float3(0, KWS_WindSpeed * 0.5, 0)).y);
	float fade = GetWaterRawFade(i.worldPos, surfaceDepthZEye, refractedSceneZEye, surfaceMask, depthAngleFix);
	half3 underwaterColor = volumeLight.xyz;
	
	#if defined(KW_FLOW_MAP_FLUIDS)
		underwaterColor = GetFluidsColor(underwaterColor, worldNormal, volumeLight, fluidsFoam, exposure);
	#endif
	underwaterColor += ComputeSSS(screenUV, GetWaterSSS(screenUV), underwaterColor, volumeLight.a > 0.5, KW_Transparent) * 10;
	
	UNITY_BRANCH
	if(KWS_UseIntersectionFoam) underwaterColor = ComputeIntersectionFoam(underwaterColor, worldNormal, fade, i.worldPos, volumeLight, KWS_DyeColor.xyz, exposure);
	
	UNITY_BRANCH
	if(KWS_UseOceanFoam) underwaterColor = ComputeOceanFoam(foam, worldNormal, underwaterColor, fade, i.worldPos, volumeLight, KWS_DyeColor.xyz, exposure);
	
	
	/////////////////////////////////////////////////////////////  end underwater  ////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	

	/////////////////////////////////////////////////////////////  REFLECTION  ////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	float3 reflDir = reflect(-viewDir, float3(tangentNormal.x, 1, tangentNormal.z));
	reflDir.y *= dot(reflDir, float3(0, 1, 0)) <= 0 ? -1 : 1;

	float3 reflection = 0;

	#if defined(KWS_SSR_REFLECTION) || defined(KWS_USE_PLANAR_REFLECTION)
		float2 refl_uv = GetScreenSpaceReflectionUV(reflDir, screenUV + tangentNormal.xz * 0.5);
	#endif

	
	#if KWS_USE_PLANAR_REFLECTION
		reflection = GetPlanarReflectionWithClipOffset(refl_uv) * exposure;
	#else

		#if KWS_USE_REFLECTION_PROBES
			reflection = KWS_GetReflectionProbeEnv(screenUV, surfaceDepthZEye, i.worldPos, reflDir, KWS_SkyLodRelativeToWind, exposure);
		#else 
			reflection = KWS_GetSkyColor(reflDir, KWS_SkyLodRelativeToWind, exposure);
		#endif

	#endif

	#if KWS_SSR_REFLECTION
		float4 ssrReflection = GetScreenSpaceReflectionWithStretchingMask(refl_uv, i.worldPos);
		reflection = lerp(reflection, ssrReflection.rgb, ssrReflection.a);
	#endif
	
	
	reflection *= surfaceMask;

	//return float4(reflection, 1);
	/////////////////////////////////////////////////////////////  end reflection  ////////////////////////////////////////////////////////////////////////////////////////////////////////
	

	#if USE_SHORELINE
		reflection = ApplyShorelineWavesReflectionFix(reflDir, reflection, underwaterColor);
	#endif
	
	half waterFresnel = ComputeWaterFresnel(worldNormal, viewDir);
	waterFresnel *= surfaceMask;
	half3 finalColor = lerp(underwaterColor, reflection, waterFresnel);
	
	#if REFLECT_SUN
		finalColor += ComputeSunlight(worldNormal, viewDir, GetMainLightDir(), GetMainLightColor(exposure), volumeLight.a, surfaceDepthZEye, _ProjectionParams.z, KW_Transparent);
		//finalColor += sunReflection * (1 - fogOpacity);
	#endif
	
	half3 fogColor;
	half3 fogOpacity;
	GetInternalFogVariables(i.pos, viewDir, surfaceDepthZEye, i.screenPos.z, fogColor, fogOpacity);
	finalColor = ComputeInternalFog(finalColor, fogColor, fogOpacity);
	finalColor = ComputeThirdPartyFog(finalColor, i.worldPos, screenUV, i.screenPos.z);

	if (KWS_UseWireframeMode) finalColor = ComputeWireframe(i.surfaceMask.xyz, finalColor);
	
	half surfaceTensionFade = GetSurfaceTension(sceneZEye, surfaceDepthZEye);
	finalColor += srpBatcherFix;

	return float4(finalColor, surfaceTensionFade);
}


struct FragmentOutput
{
	half4 pass1 : SV_Target0;
	half2 pass2 : SV_Target1;
};

FragmentOutput  fragDepth(v2fDepth i, float facing : VFACE) : SV_Target
{
	UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
	FragmentOutput o = (FragmentOutput)0;

	float facingColor = 0.75 - facing * 0.25;
	float2 screenUV = i.screenPos.xy / i.screenPos.w;
	float z = i.screenPos.z / i.screenPos.w;
	float sceneDepth = GetSceneDepth(screenUV);

	
	#ifdef KWS_PRE_PASS_BACK_FACE
		float mask = i.surfaceMask.x < 0.9999 ? 0.1 : 1;
	#else
		float mask = facing > 0 ? 0.25 : 0.75;
		if(i.surfaceMask.x < 0.9999) 
		{
			mask = facing > 0.0 ? 0.1 : 1;
		}
		
		//if(KWS_MeshType != KWS_MESH_TYPE_INFINITE_OCEAN && worldPos.y < KWS_OceanLevel + 0.1) discard;
	
	#endif

	#ifdef KWS_VOLUME_MASK
		//float3 sdfMask = GetWaterVolumePrePass(i.worldPosRefracted, id);
		//if(sdfMask.z > 0) mask = saturate(facing);
		//if(sdfMask.x >= WATER_VOLUME_PRE_PASS_MAX_VALUE) discard;
	#else 
		
		//#if !defined(KWS_USE_UNDERWATER)
		//	if(LinearEyeDepthUniversal(sceneDepth) < LinearEyeDepthUniversal(z)) discard;
		//#endif

	#endif


	float3 wavesNormalFoam;
	#if defined(KW_FLOW_MAP) || defined(KW_FLOW_MAP_EDIT_MODE)
		wavesNormalFoam = GetFftWavesNormalFoamWithFlowmap(i.worldPos);
	#else
		wavesNormalFoam = GetFftWavesNormalFoam(i.worldPos);
	#endif
	float3 tangentNormal = float3(wavesNormalFoam.x, 1, wavesNormalFoam.z); 

	float3 tangentNormalScatter = GetFftWavesNormalLod(i.worldPos, KWS_WATER_SSR_NORMAL_LOD);

	#if KW_DYNAMIC_WAVES
		float3 dynamicWavesNormal = GetDynamicWavesNormals(i.worldPos);
		tangentNormal = KWS_BlendNormals(tangentNormal, dynamicWavesNormal);
	#endif
	
	float3 worldNormal = KWS_BlendNormals(tangentNormal, i.worldNormal);
	worldNormal *= i.surfaceMask.x > 0.9999 ?  1 : 0;

	float3 viewDir = GetWorldSpaceViewDirNorm(i.worldPos);
	float3 lightDir = GetMainLightDir();
	float distanceToCamera = GetWorldToCameraDistance(i.worldPos);
	float sssDistance = (1 - saturate(distanceToCamera * 0.001)) * saturate((distanceToCamera - 2) * 0.25);

	float3 H = normalize(lightDir + tangentNormalScatter * float3(-1, 1, -1) * 3);
    float I = saturate(KWS_Pow3(saturate(dot(viewDir, -H))) * 100);
	float sunAngleAttenuation = saturate(GetVolumeLightSunAngleAttenuation(lightDir) * 20);
	float surfaceAttenuation = saturate((dot(worldNormal, float3(0, 1, 0)) - 0.9) * 10);
	float localWaveAttenuation = saturate(i.localHeightAndTensionMask.x);
	float sss = I * surfaceAttenuation * sssDistance * sunAngleAttenuation * localWaveAttenuation;
	
	float waterInstanceID = KWS_WaterInstanceID * KWS_WATER_MASK_DECODING_VALUE;

	half tensionMask = 0;
	#ifdef KWS_USE_HALF_LINE_TENSION
		tensionMask = abs(i.localHeightAndTensionMask.y) * KWS_InstancingWaterScale.y * lerp(40, 10, KWS_UnderwaterHalfLineTensionScale);
		if(tensionMask >= 0.99) tensionMask = ((1.2 - tensionMask) * 5);
		if(i.surfaceMask.x > 0.9999 || facing < 0 || z <= sceneDepth) tensionMask = 0;
		tensionMask *= 1-saturate(distanceToCamera * 0.1);
	#endif

	o.pass1 = half4(waterInstanceID, mask, sss, tensionMask);
	o.pass2 = worldNormal.xz;

	return o;
}