Shader "Hidden/KriptoFX/KWS/Underwater"
{
	Properties
	{
		[HideInInspector]KWS_StencilMaskValue ("KWS_StencilMaskValue", Int) = 32
	}

	SubShader
	{
		Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
	
		ZWrite Off
		Cull Off

		Stencil
		{
			Ref [KWS_StencilMaskValue]
			ReadMask [KWS_StencilMaskValue]
			Comp Greater
			Pass keep
		}


		Pass
		{
			HLSLPROGRAM
			#pragma vertex vertUnderwater
			#pragma fragment fragUnderwater
			#pragma target 4.6

			#pragma multi_compile_fragment _ USE_AQUARIUM_MODE
			#pragma multi_compile_fragment _ KWS_USE_VOLUMETRIC_LIGHT
			#pragma multi_compile_fragment _ USE_PHYSICAL_APPROXIMATION_COLOR USE_PHYSICAL_APPROXIMATION_SSR
			#pragma multi_compile_fragment _ KWS_USE_HALF_LINE_TENSION
			#pragma multi_compile_fragment _ KWS_CAMERA_UNDERWATER
			#pragma multi_compile_fragment _ KWS_USE_AQUARIUM_RENDERING

			#include "../../PlatformSpecific/Includes/KWS_HelpersIncludes.cginc"
			
			DECLARE_TEXTURE(_SourceRT);
			float4 KWS_Underwater_RTHandleScale;
			float4 _SourceRTHandleScale;
			float4 _SourceRT_TexelSize;


			float MaskToAlpha(float mask)
			{
				return saturate(mask * mask * mask * 20);
			}

			struct vertexInput
			{
				uint vertexID : SV_VertexID;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct vertexOutput
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				UNITY_VERTEX_OUTPUT_STEREO
			};


			vertexOutput vertUnderwater(vertexInput v)
			{
				vertexOutput o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				o.vertex = GetTriangleVertexPosition(v.vertexID);
				o.uv = GetTriangleUVScaled(v.vertexID);
				return o;
			}

			half4 fragUnderwater(vertexOutput i) : SV_Target
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

				float2 uv = i.uv;

				#if KWS_USE_HALF_LINE_TENSION
					float waterHalflineMask = GetWaterHalfLineTensionMask(uv - float2(0, _SourceRT_TexelSize.y * 5));
					float halfLineUvOffset = -waterHalflineMask * 0.25 + waterHalflineMask * waterHalflineMask * 0.25;
					uv.y -= halfLineUvOffset;
				#endif
				
				float sceneZ = GetSceneDepth(uv);
				float waterMask = GetWaterMask(uv);
				float2 volumeDepth = GetWaterVolumeDepth(uv, sceneZ, waterMask);
				
				//return float4(GetVolumetricLight(uv).xyz, 1);

				bool surfaceMask = (abs(waterMask - 0.75) < 0.1) && (volumeDepth.y > sceneZ); //bool zero VGPR cost
				
				#ifdef USE_AQUARIUM_MODE
					float aquariumMask = GetWaterAquariumBackfaceMask(uv);
					float backfaceDepth = GetWaterBackfaceDepth(uv);
					if(aquariumMask > 0)
					{
						if(abs(waterMask - 0.25) < 0.1 || backfaceDepth < sceneZ) aquariumMask = 0;
						if(aquariumMask > 0.1) surfaceMask = true;
					}
				#endif
				
				float alpha = volumeDepth.x > 0 && waterMask > 0.5 ? 1 : 0;
				#ifdef USE_AQUARIUM_MODE
					alpha = aquariumMask > 0.1 ? 1 : alpha;
				#endif
				#if KWS_USE_HALF_LINE_TENSION
					alpha = saturate(alpha + waterHalflineMask * 10);
				#endif

				if(alpha == 0) discard;
			
				float3 worldPos = GetWorldSpacePositionFromDepth(uv, sceneZ); //todo add real water wave offset
				uint waterID = GetWaterID(uv);

				half3 normal = GetWaterNormals(i.uv.xy) * surfaceMask;
				float2 refractionUV = uv.xy + normal.xz;
				half3 refraction = GetSceneColor(refractionUV);
				
				#if defined(USE_PHYSICAL_APPROXIMATION_COLOR) || defined(USE_PHYSICAL_APPROXIMATION_SSR)
					float3 waterSurfaceWorldPos = GetWorldSpacePositionFromDepth(uv, volumeDepth.y);
					float3 worldViewDir = GetWorldSpaceViewDirNorm(waterSurfaceWorldPos);
					float distanceToSurface = saturate((waterSurfaceWorldPos.y - GetCameraAbsolutePosition().y) * 0.3);
					//float3 refractedRay = refract(-worldViewDir, normal, lerp(0.95, KWS_WATER_IOR, distanceToSurface));
					float3 refractedRay = refract(-worldViewDir, normal, KWS_WATER_IOR);

					float refractedMask = 1 - clamp(-refractedRay.y * 100, 0, 1);
					refractedMask *= surfaceMask;
				
					float3 reflection = KWS_TurbidityColorArray[waterID] * 0.05;
				#endif

				#ifdef USE_PHYSICAL_APPROXIMATION_SSR
					float3 reflDir = reflect(-worldViewDir, normal);
					float2 refl_uv = GetScreenSpaceReflectionUV(reflDir, uv + normal.xz * 0.5);
					float4 ssrReflection = GetScreenSpaceReflection(refl_uv, waterSurfaceWorldPos);
					
					reflection = lerp(refraction.xyz, ssrReflection.xyz, ssrReflection.a);
					reflection = lerp(reflection, 0, distanceToSurface);
					reflection = lerp(reflection.xyz, ssrReflection.xyz, ssrReflection.a);

				#endif
				
				#if defined(USE_PHYSICAL_APPROXIMATION_COLOR) || defined(USE_PHYSICAL_APPROXIMATION_SSR)
					refraction = lerp(refraction, reflection, refractedMask);
				#endif
				
				
				float waterHeight = KWS_WaterPositionArray[waterID].y;
				float transparent = KWS_TransparentArray[waterID];
				float3 turbidityColor = KWS_TurbidityColorArray[waterID];
				float3 dyeColor = KWS_DyeColorArray[waterID];

				transparent = clamp(transparent + KWS_UnderwaterTransparentOffsetArray[waterID], 1, KWS_MAX_TRANSPARENT * 2);

				float3 volLight = GetVolumetricLightWithAbsorbtion(uv, uv, transparent, turbidityColor, dyeColor, refraction, volumeDepth, waterID, GetExposure(), 0).xyz;
				


				#if KWS_USE_HALF_LINE_TENSION
					volLight = lerp(volLight, refraction * volLight * 2 * waterHalflineMask * waterHalflineMask + volLight * 0.5, waterHalflineMask);
				#endif


				return float4(volLight, alpha);
			}

			ENDHLSL
		}
	}
}