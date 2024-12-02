Shader "Hidden/KriptoFX/KWS/VolumetricLighting"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" { }
	}
	SubShader
	{
		// No culling or depth
		Cull Off
		ZWrite Off
		ZTest Always

		Pass
		{
			HLSLPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma target 4.6

			#pragma multi_compile_fragment _ USE_CAUSTIC USE_ADDITIONAL_CAUSTIC
			#pragma multi_compile_fragment _ KWS_USE_AQUARIUM_RENDERING

			#pragma multi_compile_fragment _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
			#pragma multi_compile_fragment _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
			#pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
			
			#pragma multi_compile_fragment _ _FORWARD_PLUS
			#pragma multi_compile_fragment _ _LIGHT_LAYERS

			#include "../PlatformSpecific/Includes/KWS_HelpersIncludes.cginc"
			#include "../Common/CommandPass/KWS_VolumetricLight_Common.cginc"

			int KWS_AdditionalLightsCount;
			
			int KWS_GetAdditionalLightsCount()
			{
				#if USE_FORWARD_PLUS
					return 0;
				#else
					return KWS_AdditionalLightsCount;
				#endif
			}

			inline void IntegrateAdditionalLight(RaymarchData raymarchData, inout float3 scattering, inout float transmittance, float atten, float3 lightPos, float3 step, inout float3 currentPos)
			{
				float3 posToLight = normalize(currentPos - lightPos.xyz);
				
				#if defined(USE_ADDITIONAL_CAUSTIC)
					if (lightPos.y > raymarchData.waterHeight)
					{
						atten += atten * RaymarchCaustic(raymarchData, currentPos, posToLight);
					}
				#endif
				
				IntegrateLightSlice(scattering, transmittance, atten, raymarchData);
				currentPos += step;
			}

			void RayMarchDirLight(RaymarchData raymarchData, inout RaymarchResult result)
			{
				result.DirLightScattering = 0;
				result.DirLightSurfaceShadow = 1;
				result.DirLightSceneShadow = 1;
				
				float3 finalScattering = 0;
				float transmittance = 1;

				float3 reflectedStep = reflect(raymarchData.rayDir, float3(0, -1, 0)) * (raymarchData.rayLength / KWS_RayMarchSteps);

				
				Light light = GetMainLight();

				#ifdef _LIGHT_LAYERS
					if (IsMatchingLightLayer(light.layerMask, KWS_WaterLightLayerMask))
				#endif
				{
					float3 currentPos = raymarchData.currentPos;
					float3 step = raymarchData.step;

					float sunAngleAttenuation = GetVolumeLightSunAngleAttenuation(light.direction.xyz);
					finalScattering = GetAmbientColor(GetExposure()) * 0.5;
					finalScattering *= GetVolumeLightInDepthTransmitance(raymarchData.waterHeight, currentPos.y, raymarchData.transparent, raymarchData.waterID);
					finalScattering *= sunAngleAttenuation;

					UNITY_LOOP
					for (uint i = 0; i < KWS_RayMarchSteps; ++i)
					{
						if (length(currentPos - raymarchData.rayStart) > raymarchData.rayLengthToSceneZ) break;
						if (length(currentPos - raymarchData.rayStart) > raymarchData.rayLengthToWaterZ) step = reflectedStep;
						
						float atten = MainLightRealtimeShadow(TransformWorldToShadowCoord(currentPos));
						
						#if defined(USE_CAUSTIC) || defined(USE_ADDITIONAL_CAUSTIC)
							atten += atten * RaymarchCaustic(raymarchData, currentPos, light.direction);
						#endif
						atten *= sunAngleAttenuation;
						atten *= GetVolumeLightInDepthTransmitance(raymarchData.waterHeight, currentPos.y, raymarchData.transparent, raymarchData.waterID);
						
						IntegrateLightSlice(finalScattering, transmittance, atten, raymarchData);
						currentPos += step ;
					}
					
					finalScattering *= light.color * raymarchData.tubidityColor;
					
					result.DirLightSurfaceShadow = MainLightRealtimeShadow(TransformWorldToShadowCoord(raymarchData.rayStart));
					#if defined(USE_CAUSTIC) || defined(USE_ADDITIONAL_CAUSTIC)
						result.DirLightSceneShadow = MainLightRealtimeShadow(TransformWorldToShadowCoord(raymarchData.rayEnd));
					#endif
				}
				
				result.DirLightScattering = finalScattering;
			}

			void RayMarchAdditionalLights(RaymarchData raymarchData, inout RaymarchResult result)
			{
				result.AdditionalLightsScattering = 0;
				result.AdditionalLightsSceneAttenuation = 0;

				InputData inputData;
				inputData.normalizedScreenSpaceUV = raymarchData.uv;
				inputData.positionWS = raymarchData.currentPos;

				#if defined(_ADDITIONAL_LIGHTS) || defined(_ADDITIONAL_LIGHTS_VERTEX)
					
					//#if USE_FORWARD_PLUS
					//	for (uint lightIndex = 0; lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); lightIndex++)
					//	{
					//		float3 scattering = 0;
					//		float transmittance = 1;
					//		float3 currentPos = raymarchData.currentPos;
					//		result.AdditionalLightsSceneAttenuation = max(result.AdditionalLightsSceneAttenuation, GetAdditionalLight(lightIndex, raymarchData.rayEnd).distanceAttenuation);
					//		Light light;
					//		UNITY_LOOP
					//		for (uint i = 0; i < KWS_RayMarchSteps; ++i)
					//		{
					//			if(length(currentPos - raymarchData.rayStart) > raymarchData.rayLengthToSceneZ) break;
					//			light = GetAdditionalLight(lightIndex, currentPos, 1.0);
					//			IntegrateAdditionalLight(raymarchData, scattering, transmittance, light.distanceAttenuation * light.shadowAttenuation, light.direction, currentPos);
					//		    currentPos += step;
					//}
					//		result.AdditionalLightsScattering += scattering * light.color * raymarchData.tubidityColor;
					
					//	}
					//#endif
					
					//uint pixelLightCount = GetAdditionalLightsCount(); AdditionalLightsCount is set on per-object basis by unity's rendering code.
					//Its just gonna return you the number of lights that the last object rendered was affected by.
					uint pixelLightCount = KWS_GetAdditionalLightsCount();
					float3 reflectedStep = reflect(raymarchData.rayDir, float3(0, -1, 0)) * (raymarchData.rayLength / KWS_RayMarchSteps);

					LIGHT_LOOP_BEGIN(pixelLightCount)
					
					float3 scattering = 0;
					float transmittance = 1;
					float3 currentPos = raymarchData.currentPos;
					result.AdditionalLightsSceneAttenuation = max(result.AdditionalLightsSceneAttenuation, GetAdditionalLight(lightIndex, raymarchData.rayEnd).distanceAttenuation);
					Light light;
					float3 step = raymarchData.step;

					UNITY_LOOP
					for (uint i = 0; i < KWS_RayMarchSteps; ++i)
					{
						if (length(currentPos - raymarchData.rayStart) > raymarchData.rayLengthToSceneZ) break;
						if (length(currentPos - raymarchData.rayStart) > raymarchData.rayLengthToWaterZ) step = reflectedStep;

						light = GetAdditionalPerObjectLight(lightIndex, currentPos);
						float atten = AdditionalLightRealtimeShadow(lightIndex, currentPos, light.direction) * light.distanceAttenuation;
						IntegrateAdditionalLight(raymarchData, scattering, transmittance, atten, light.direction, step, currentPos);
					}
					result.AdditionalLightsScattering += scattering * light.color * raymarchData.tubidityColor;

					LIGHT_LOOP_END


				#endif
			}


			void frag(vertexOutput i, out half3 volumeLightColor : SV_Target0, out half3 additionalData : SV_Target1)
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
				float waterMask = GetWaterMask(i.uv);
				if (waterMask == 0) discard;

				RaymarchData raymarchData = InitRaymarchData(i, waterMask);
				RaymarchResult raymarchResult = (RaymarchResult)0;

				RayMarchDirLight(raymarchData, raymarchResult);
				RayMarchAdditionalLights(raymarchData, raymarchResult);
				
				volumeLightColor = raymarchResult.DirLightScattering + raymarchResult.AdditionalLightsScattering;
				additionalData = float3(raymarchResult.DirLightSurfaceShadow, raymarchResult.DirLightSceneShadow, raymarchResult.AdditionalLightsSceneAttenuation);

				AddTemporalAccumulation(raymarchData.rayEnd, volumeLightColor);
			}

			ENDHLSL
		}
	}
}