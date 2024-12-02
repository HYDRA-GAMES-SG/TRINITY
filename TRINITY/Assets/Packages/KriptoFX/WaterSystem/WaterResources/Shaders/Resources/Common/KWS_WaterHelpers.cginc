#define UNITY_PI            3.14159265359f

float4 SampleBilinearSmooth(Texture2D tex, SamplerState state, float2 uv, float4 texelSize)
{
	float2 textureResolution = texelSize.zw;
	uv = uv * textureResolution + 0.5;
	float2 iuv = floor(uv);
	float2 fuv = frac(uv);
	uv = iuv + fuv * fuv * (3.0 - 2.0 * fuv);
	uv = (uv - 0.5) / textureResolution;
	return tex.Sample(state, uv);
}

float4 SampleBilinearSmoothLevel(Texture2D tex, SamplerState state, float2 uv, float4 texelSize)
{
	float2 textureResolution = texelSize.zw;
	uv = uv * textureResolution + 0.5;
	float2 iuv = floor(uv);
	float2 fuv = frac(uv);
	uv = iuv + fuv * fuv * (3.0 - 2.0 * fuv);
	uv = (uv - 0.5) / textureResolution;
	return tex.SampleLevel(state, uv, 0);
}



inline half4 SampleSupersampledMip(sampler2D tex, float2 uv, float2 texSize, float bias)
{
	float2 dx = ddx(uv);
	float2 dy = ddy(uv);// manually calculate the per axis mip level, clamp to 0 to 1
	// and use that to scale down the derivatives
	dx *= saturate(
		0.5 * log2(dot(dx * texSize, dx * texSize))
	);
	dy *= saturate(
		0.5 * log2(dot(dy * texSize, dy * texSize))
	);// rotated grid uv offsets
	float2 uvOffsets = float2(0.125, 0.375);
	float4 offsetUV = float4(0.0, 0.0, 0.0, bias);// supersampled using 2x2 rotated grid

	half4 color;
	offsetUV.xy = uv + uvOffsets.x * dx + uvOffsets.y * dy;
	color = tex2Dbias(tex, offsetUV);
	offsetUV.xy = uv - uvOffsets.x * dx - uvOffsets.y * dy;
	color += tex2Dbias(tex, offsetUV);
	offsetUV.xy = uv + uvOffsets.y * dx - uvOffsets.x * dy;
	color += tex2Dbias(tex, offsetUV);
	offsetUV.xy = uv - uvOffsets.y * dx + uvOffsets.x * dy;
	color += tex2Dbias(tex, offsetUV);
	color *= 0.25;
	return color;
}

inline half4 SampleSupersampled(sampler2D tex, float2 uv, float2 texelSize, float bias)
{
	half4 color;
	float3 texelOffset = float3(texelSize.xy, 0);
	color = tex2Dbias(tex, float4(uv + texelOffset.xz, 0, bias));
	color += tex2Dbias(tex, float4(uv - texelOffset.xz, 0, bias));
	color += tex2Dbias(tex, float4(uv + texelOffset.zy, 0, bias));
	color += tex2Dbias(tex, float4(uv - texelOffset.zy, 0, bias));
	color *= 0.25;

	//color = color * 0.5 + tex2Dbias(tex, float4(uv, 0, bias)) * 0.5;

	return color;
}

inline half4 SampleSupersampledLod(sampler2D tex, float2 uv, float2 texelSize)
{
	half4 color;
	float3 texelOffset = float3(texelSize.xy, 0);
	color = tex2Dlod(tex, float4(uv + texelOffset.xz, 0, 0));
	color += tex2Dlod(tex, float4(uv - texelOffset.xz, 0, 0));
	color += tex2Dlod(tex, float4(uv + texelOffset.zy, 0, 0));
	color += tex2Dlod(tex, float4(uv - texelOffset.zy, 0, 0));
	color *= 0.25;
	return color;
}

inline half ComputeSSS(half lh, float3 normalLod, half3 lightDir)
{
	return saturate(1 - lh) * max(0, dot(normalLod, -lightDir * float3(1, 0.1, 1)));
}


inline void ComputeNormalsUsingDynamicRipples(sampler2D ripplesNormTex, sampler2D ripplesNormTexLast, sampler2D ripplesTex, float2 uv, inout half3 normal, inout half3 normalLod, inout float2 mainUV)
{
	half2 ripples = tex2D(ripplesTex, uv).xy;
	half2 ripplesPrev = tex2D(ripplesTex, uv).xy;
	ripples = lerp(ripples, ripplesPrev, 0.5).xy;
	half ripplesMask = tex2D(ripplesTex, uv).r;
	
	mainUV += ripples.xy / 10;
	//normal.xz += ripples.xy;
	
	normal.xz = lerp(normal.xz, ripples.xy, abs(ripplesMask));
	//normNotDetailed.xz += ripplesNormal * 2;
	normalLod.xz += ripples.xy;
	//return saturate(1 - saturate(abs(ripples.x * 5)));

}


inline half3 ViewNormal(half3 worldNorm)
{
	float3 viewNorm = mul((float3x3)UNITY_MATRIX_V, worldNorm);
	return viewNorm;
}


inline half SelfSmithJointGGXVisibilityTerm(half NdotL, half NdotV, half roughness)
{
	half a = roughness;
	half lambdaV = NdotL * (NdotV * (1 - a) + a);
	half lambdaL = NdotV * (NdotL * (1 - a) + a);

	return 0.5f / (lambdaV + lambdaL + 1e-5f);
}


inline half SelfGGXTerm(half NdotH, half roughness)
{
	half a2 = roughness * roughness;
	half d = (NdotH * a2 - NdotH) * NdotH + 1.0f; // 2 mad
	return 0.31830988618f * a2 / (d * d + 1e-7f); // This function is not intended to be running on Mobile,
	// therefore epsilon is smaller than what can be represented by half
}


//half3 Refract(half3 viewDir, half3 normal, float ior)
//{
//	half nv = dot(normal, viewDir);
//	half v2 = dot(viewDir, viewDir);
//	half knormal = (sqrt(((ior * ior - 1) * v2) / (nv * nv) + 1.0) - 1.0) * nv;
	
//	return (knormal * normal);
//}

inline float3 ScreenToWorld(float2 UV, float depth, float4x4 projToView, float4x4 viewToWorld)
{
	float2 uvClip = UV * 2.0 - 1.0;
	float4 clipPos = float4(uvClip, depth, 1.0);
	float4 viewPos = mul(projToView, clipPos);
	viewPos /= viewPos.w;
	float3 worldPos = mul(viewToWorld, viewPos).xyz;
	return worldPos;
}

float Fresnel_IOR(float3 viewDir, float3 normal, float ior)
{
	float cosi = clamp(-1, 1, dot(viewDir, normal));
	float etai = 1, etat = ior;
	if (cosi > 0)
	{
		float temp = etat;
		etat = etai;
		etai = temp;
	}
	
	float sint = etai / etat * sqrt(max(0.f, 1 - cosi * cosi));
	
	if (sint >= 1)
	{
		return 1;
	}
	else
	{
		float cost = sqrt(max(0.f, 1 - sint * sint));
		cosi = abs(cosi);
		float Rs = ((etat * cosi) - (etai * cost)) / ((etat * cosi) + (etai * cost));
		float Rp = ((etai * cosi) - (etat * cost)) / ((etai * cosi) + (etat * cost));
		return (Rs * Rs + Rp * Rp) / 2;
	}
	// As a consequence of the conservation of energy, transmittance is given by:
	// kt = 1 - kr;

}

inline half ComputeSpecular(half nl, half nv, half nh, half viewDistNormalized, half smoothness)
{
	half V = SelfSmithJointGGXVisibilityTerm(nl, nv, smoothness);
	half D = SelfGGXTerm(nh, viewDistNormalized * 0.1 + smoothness);

	half specularTerm = V * D;
	//
	//#   ifdef UNITY_COLORSPACE_GAMMA
	//	specularTerm = sqrt(max(1e-4h, specularTerm));
	//#   endif


	specularTerm = max(0, specularTerm * nl * KWS_SunStrength);

	return specularTerm;
}

half3 ComputeUnderwaterColor(half3 refraction, half3 volumeLight, half fade, half transparent, half3 waterColor, half turbidity, half3 turbidityColor)
{
	fade = max(0, fade);
	float fadeExp = saturate(1 - exp(-5 * fade / transparent));
	half3 absorbedColor = pow(clamp(waterColor.xyz, 0.1, 0.95), 25 * fade / transparent); //min range ~ 0.0  with pow(x, 70)
	half3 waterColorInDepth = pow(waterColor.xyz, 15.0) * 0.05 * volumeLight.rgb;
	half3 turbidityColorInDepth = turbidityColor * volumeLight.rgb;
	
	absorbedColor = lerp(waterColorInDepth, refraction, absorbedColor);
	turbidityColor = lerp(refraction, turbidityColorInDepth, fadeExp);
	absorbedColor = lerp(absorbedColor, turbidityColor, turbidity);

	return absorbedColor;
}

half3 ComputeUnderwaterColor(half3 refraction, half3 volumeLight, half fade, half transparent, half3 waterColor, half turbidity, half3 turbidityColor, half3 fogOpacity, half3 fogColor)
{
	fade = max(0, fade);
	float fadeExp = saturate(1 - exp(-5 * fade / transparent));
	half3 absorbedColor = pow(clamp(waterColor.xyz, 0.1, 0.95), 25 * fade / transparent); //min range ~ 0.0  with pow(x, 70)
	half3 waterColorInDepth = lerp(pow(waterColor.xyz, 15.0) * 0.05 * volumeLight.rgb, fogColor, fogOpacity);
	half3 turbidityColorInDepth = lerp(turbidityColor * volumeLight.rgb, fogColor, fogOpacity);
	
	absorbedColor = lerp(waterColorInDepth, refraction, absorbedColor);
	turbidityColor = lerp(refraction, turbidityColorInDepth, fadeExp);
	absorbedColor = lerp(absorbedColor, turbidityColor, turbidity);

	return absorbedColor;
}


half3 ComputeUnderwaterSurfaceColor(half3 volumeLight, half transparent, half3 waterColor, half turbidity, half3 turbidityColor)
{
	float depth = 70;
	half3 absorbedColor = pow(clamp(waterColor.xyz, 0.1, 0.95), depth); //min range ~ 0.0  with pow(x, 70)
	absorbedColor = lerp(absorbedColor, turbidityColor * volumeLight.rgb, turbidity);
	return absorbedColor;
}


float ComputeWaterFresnel(half3 normal, half3 viewDir)
{
	float x = 1 - saturate(dot(normal, viewDir));
	return 0.03 + 0.97 * x * x * x * x * x; //fresnel aproximation http://wiki.nuaj.net/images/thumb/1/16/Fresnel.jpg/800px-Fresnel.jpg

}

half3 ComputeSSS(float2 screenUV, float sssMask, half3 underwaterColor, half shadowMask, half KW_Transparent)
{
	float3 sssColor = dot(underwaterColor, 0.333) * 0.25 + underwaterColor * 0.75;
	return sssMask * shadowMask * sssColor * saturate(1 - KW_Transparent / 100);
}

half3 ComputeSunlight(half3 normal, half3 viewDir, float3 lightDir, float3 lightColor, half shadowMask, float viewDist, float waterFarDistance, half KW_Transparent)
{
	half3 halfDir = normalize(lightDir + viewDir);
	half nh = saturate(dot(normal, halfDir));
	half nl = saturate(dot(normal, lightDir));
	half lh = saturate(dot(lightDir, halfDir));
	half fresnel = saturate(dot(normal, viewDir));
	
	float viewDistNormalized = saturate(viewDist / (waterFarDistance * 2));
	half3 specular = ComputeSpecular(nl, fresnel, nh, viewDistNormalized, KWS_SunCloudiness);
	specular = clamp(specular * 10 - 2.5 * saturate(1 - KWS_SunCloudiness * 10), 0, KWS_SunMaxValue);
	//half sunset = saturate(0.01 + dot(lightDir, float3(0, 1, 0))) * 30;

	return shadowMask * specular * lightColor;
}


float2 GetScreenSpaceReflectionUV(float3 reflDir, float2 orthoUV)
{
	UNITY_BRANCH
	if (unity_OrthoParams.w == 1.0) return orthoUV;
	else
	{
		reflDir.y = -reflDir.y;
		float4 projected = mul(UNITY_MATRIX_VP, float4(reflDir, 0));
		float2 uv = (projected.xy / projected.w) * 0.5f + 0.5f;
	
		#ifdef UNITY_UV_STARTS_AT_TOP
			uv.y = 1 - uv.y;
		#endif

		return uv;
	}
}


inline half3 GetFluidsNormal(float3 worldPos, float2 uv, half3 normal, out half foam)
{
	foam = 0;

	float2 fluidsUV_lod1 = (worldPos.xz - KW_FluidsMapWorldPosition_lod1.xz) / KW_FluidsMapAreaSize_lod1 + 0.5;
	float fluidsMask = KWS_FluidsMask_Lod1.SampleLevel(sampler_linear_clamp, fluidsUV_lod1, 0).x;
	if(fluidsMask < 0.001) return normal;

	float2 fluidsUV_lod0 = (worldPos.xz - KW_FluidsMapWorldPosition_lod0.xz) / KW_FluidsMapAreaSize_lod0 + 0.5;
	float4 fluidsData_lod0 = KW_Fluids_Lod0.SampleLevel(sampler_linear_clamp, fluidsUV_lod0, 0);

	
	float4 fluidsData_lod1 = KW_Fluids_Lod1.SampleLevel(sampler_linear_clamp, fluidsUV_lod1, 0);

	float2 maskUV_lod0 = 1 - saturate(abs(fluidsUV_lod0 * 2 - 1));
	float lodLevelFluidMask_lod0 = saturate((maskUV_lod0.x * maskUV_lod0.y - 0.01) * 5);
	float2 maskUV_lod1 = 1 - saturate(abs(fluidsUV_lod1 * 2 - 1));
	float lodLevelFluidMask_lod1 = saturate((maskUV_lod1.x * maskUV_lod1.y - 0.01) * 5);

	float3 fluids = lerp(fluidsData_lod1.xyz, fluidsData_lod0.xyz, lodLevelFluidMask_lod0);
	fluids *= lodLevelFluidMask_lod1;

	//normal = ComputeNormalUsingFlowMap(KW_NormTex, sampler_linear_repeat, fluids.xy * KW_FluidsVelocityAreaScale * 0.75, normal, uv, _Time.y * KW_FlowMapSpeed * 0.5);

	float foamMask_lod0 = KW_FluidsFoam_Lod0.SampleLevel(sampler_linear_clamp, fluidsUV_lod0, 0).x;
	float foamMask_lod1 = KW_FluidsFoam_Lod1.SampleLevel(sampler_linear_clamp, fluidsUV_lod1, 0).x;
	float foamTex_lod0 = KW_FluidsFoamTex.SampleBias(sampler_linear_repeat, worldPos.xz / 40 + fluidsData_lod0.xy * 0.125 + float2(-_Time.x * 1, 0), -3).x;
	float foamTex_lod1 = KW_FluidsFoamTex.SampleBias(sampler_linear_repeat, worldPos.xz / 100 + fluidsData_lod1.xy * 0.25 + float2(-_Time.x * 1, 0), -3).x;

	foamMask_lod1 = min(foamMask_lod1, (fluidsData_lod1.z - 0.5));
	half foamMask = lerp(foamMask_lod1, foamMask_lod0, lodLevelFluidMask_lod0);
	half foamTex = lerp(foamTex_lod1 * 1.5, foamTex_lod0, lodLevelFluidMask_lod0);
	foam = foamMask * foamTex * lodLevelFluidMask_lod1;

	return normal;
}

inline half3 GetFluidsColor(half3 underwaterColor, float3 normal, half4 volumeScattering, half fluidsFoam, float exposure)
{
	return lerp(underwaterColor, GetVolumetricSurfaceLight(volumeScattering, normal, exposure), fluidsFoam);
}

inline half4 GetFlowmapEditor(float3 worldPos, half3 normal)
{
	float2 flowMapUV = (worldPos.xz - KW_FlowMapOffset.xz) / KW_FlowMapSize + 0.5;
	if (flowMapUV.x < 0 || flowMapUV.x > 1 || flowMapUV.y < 0 || flowMapUV.y > 1) return float4(0.5, 0, 0, 1);
	return half4(pow((normal.xz + 0.75), 7), 1, 1);
}


float3 BoxProjection(float3 direction, float3 position, float3 cubemapPosition, float3 boxMin, float3 boxMax)
{
	float3 factors = ((direction > 0 ?      boxMax : boxMin) - position) / direction;
	float scalar = min(min(factors.x, factors.y), factors.z);
	direction = direction * scalar + (position - cubemapPosition);
	return direction;
}


half3 ComputeWaterRefractRay(half3 viewDir, half3 normal, half depth)
{
	half nv = dot(normal, viewDir);
	half v2 = dot(viewDir, viewDir);
	half knormal = (sqrt(((1.7689 - 1.0) * v2) / (nv * nv) + 1.0) - 1.0) * nv;
	half3 result = depth * (viewDir + (knormal * normal));
	return result;
}



inline float2 GetRefractedUV_IOR(float3 viewDir, half3 normal, float3 worldPos, float sceneZEye, float waterSurfaceZEye, float transparent)
{
	float zFix = saturate(sceneZEye - waterSurfaceZEye);
	float aproximatedDepth = min(transparent, KWS_RefractionAproximatedDepth * zFix);
	float3 refractedRay = ComputeWaterRefractRay(-viewDir, normal, aproximatedDepth);
	refractedRay.y *= 0.4; //fix information lost in the near camera
	
	float4 refractedClipPos = mul(UNITY_MATRIX_VP, float4(GetCameraRelativePosition(worldPos + refractedRay), 1.0));
	float4 refractionScreenPos = ComputeScreenPos(refractedClipPos);
	float2 uv = refractionScreenPos.xy / refractionScreenPos.w;

	/*float2 overflowUV = abs(uv * 2 - 1);
	float overflowFix = saturate(max(overflowUV.x, overflowUV.y) - 0.6);
	overflowFix *= overflowFix;
	overflowFix = lerp(0, overflowFix, zFix);
	uv = lerp(uv, uv * 0.5 + 0.25, overflowFix);*/
	
	if(uv.x >= 0.995) uv.x = 1.99-uv.x;
	if(uv.y >= 0.995) uv.y = 1.99-uv.y;
	if(uv.y <= 0) uv.y = -uv.y;
	if(uv.x <= 0) uv.x = -uv.x;
	
    return uv;
}

inline float2 GetRefractedUV_Simple(float2 uv, half3 normal)
{
	return uv + normal.xz * KWS_RefractionSimpleStrength * 0.5;
}

inline half GetSurfaceTension(float zEye, float surfaceZEye)
{
	return saturate((zEye - surfaceZEye) * 8);
}

inline half GetWaterRawFade(float3 worldPos, float surfaceDepthZEye, float refractedSceneZEye, half surfaceMask, half depthAngleFix)
{
	return (refractedSceneZEye - surfaceDepthZEye) * lerp(depthAngleFix, 1, unity_OrthoParams.w);
}

inline void FixRefractionSurfaceLeaking(float surfaceDepthZEye, float sceneZ, float sceneZEye, float2 screenUV, inout float refractedSceneZ, inout float refractedSceneZEye, inout float2 refractionUV)
{
	if(surfaceDepthZEye > refractedSceneZEye) 
	{
		refractedSceneZ = sceneZ;
		refractedSceneZEye = sceneZEye;
		refractionUV = screenUV;
	}
}

inline half3 ApplyShorelineWavesReflectionFix(float3 reflDir, half3 reflection, half3 underwaterColor)
{
	float r = 1 - saturate(dot(reflDir, float3(0, 1, 0)));
	return lerp(reflection, max(underwaterColor, reflection), KWS_Pow5(r));
}

inline float3 ComputeWireframeInterpolators(float decodedColor)
{
	if(decodedColor < -0.1) return float3(-1, -1, -1);
	else if(decodedColor < 0.01) return float3(1, 0, 0);
	else if(decodedColor < 0.51) return float3(0, 1, 0);
	return float3(0, 0, 1);
}

inline float3 ComputeWireframe(float3 baryColor, float3 sourceColor)
{
	float minBary = min(baryColor.x, min(baryColor.y, baryColor.z));
	float delta = fwidth(minBary) * 0.5;
	return minBary > delta ? sourceColor : float3(0.95, 0.95, 0.95);
}

inline float3 ComputeIntersectionFoam(float3 underwaterColor, float3 normal, float fade, float3 worldPos, float4 volumeScattering, float3 waterColor, float exposure)
{
	float2 foamMask = KWS_IntersectionFoamTex.SampleLevel(sampler_linear_repeat, worldPos.xz * KWS_IntersectionFoamFadeSize.y, 0).xy;
	float foamDepth1 = 1 - saturate(fade * KWS_IntersectionFoamFadeSize.x);
	float foamDepth2 = 1 - saturate(fade * KWS_IntersectionFoamFadeSize.x * 3);
	float foam = max(foamDepth1 * foamMask.x, foamDepth2 * foamMask.y);
	foam = saturate(foam*5);
	float3 ambient = GetAmbientColor(exposure) * 0.5;
	float3 dirLight = saturate(dot(normal, GetMainLightDir())) * GetMainLightColor(exposure);

	float3 finalColor = foam * KWS_IntersectionFoamColor.xyz * GetVolumetricSurfaceLight(volumeScattering, normal, exposure);
	underwaterColor = lerp(underwaterColor, finalColor, saturate(foam) * KWS_IntersectionFoamColor.a);

	return underwaterColor;
}

inline float3 ComputeOceanFoam(float foamMask, float3 normal, float3 underwaterColor, float fade, float3 worldPos, float4 volumeScattering, float3 waterColor, float exposure)
{
	UNITY_BRANCH
	if(KWS_WindSpeed < FOAM_MIN_WIND) return underwaterColor;
	else
	{
		float2 foamTex;
		//#if defined(KW_FLOW_MAP) || defined(KW_FLOW_MAP_EDIT_MODE)
		//	foamTex = SampleTextureWithFlowmap(worldPos, KWS_OceanFoamTex, sampler_linear_repeat, KWS_OceanFoamStrengthSize.yy).xy;
		//#else
			foamTex = KWS_OceanFoamTex.SampleLevel(sampler_linear_repeat, worldPos.xz * KWS_OceanFoamStrengthSize.y, 0).xy;
		//#endif

		float3 foam = max(foamMask * foamTex.x, foamMask * foamTex.y * 0.25);
		float3 bubblesFoam = foamMask * waterColor.xyz;
		foam = saturate(foam * 0.7 + bubblesFoam * 0.3);

		float3 ambient = GetAmbientColor(exposure) * 0.5;
		float3 dirLight = saturate(dot(normal, GetMainLightDir())) * GetMainLightColor(exposure);

		float3 finalColor = foam * KWS_OceanFoamColor.xyz * GetVolumetricSurfaceLight(volumeScattering, normal, exposure);

		underwaterColor = lerp(underwaterColor, max(underwaterColor * 0.5, finalColor), saturate(foam) * KWS_OceanFoamColor.a);

		return underwaterColor;
	}
}

float GetAnisoScaleRelativeToWind(float windSpeed)
{
	float normalizedWind = saturate((windSpeed - 0.1) / 10.0);
	return (normalizedWind) * KWS_AnisoReflectionsScale;
}

float4 ObjectToClipPosOrtho(float4 vertex)
{
	float3 worldPos = GetCameraRelativePositionOrtho(mul(UNITY_MATRIX_M, float4(vertex.xyz, 1.0)).xyz);
	return mul(KWS_MATRIX_VP_ORTHO, float4(worldPos, 1));
}
