Shader "Hidden/KriptoFX/KWS/ShorelineWaves"
{
	SubShader
	{
		Pass
		{
			Blend One One
			ZTest Always Cull Off ZWrite Off

			HLSLPROGRAM

			#define _FPS 18.0
			#define Deg2Rad 0.01745329f

			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing

			#include "../../PlatformSpecific/Includes/KWS_HelpersIncludes.cginc"
			
			Texture2D KWS_ShorelineDisplacement;
			Texture2D KWS_ShorelineNormal;
			Texture2D KWS_ShorelineAlpha;

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				uint instanceID : SV_InstanceID;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				//float3 worldPos : TEXCOORD1;
				uint instanceID : TEXCOORD1;
			};

			struct FragmentOutput
			{
				half4 dest0 : SV_Target0;
				half4 dest1 : SV_Target1;
			};

			uint _ShorelineBake_mrtBufferIdx;
			int KWS_InstanceOffset;

			float2 GetAnimatedUV(float2 uv, int _ColumnsX, int _RowsY, float FPS, float time)
			{
				float2 size = float2(1.0f / _ColumnsX, 1.0f / _RowsY);
				uint totalFrames = _ColumnsX * _RowsY;
				uint index = time * 1.0f * FPS;
				uint indexX = index % _ColumnsX;
				uint indexY = floor((index % totalFrames) / _ColumnsX);

				float2 offset = float2(size.x * indexX, -size.y * indexY);
				float2 newUV = uv * size;
				newUV.y = newUV.y + size.y * (_RowsY - 1);

				return newUV + offset;
			}

			//float ComputeWaterOrthoDepth(float3 worldPos)
			//{
			//	float2 depthUV = (worldPos.xz - KWS_ShorelineAreaPos.xz) / KW_ShorelineDepthOrthoSize + 0.5;
			//	if (depthUV.x < 0.001 || depthUV.x > 0.999 || depthUV.y < 0.001 || depthUV.y > 0.999) return 0;
			//	float terrainDepth = KW_ShorelineDepthTex.SampleLevel(sampler_linear_clamp, depthUV, 0).r * KW_ShorelineDepth_Near_Far_Dist.z - KW_ShorelineDepth_Near_Far_Dist.y + KWS_ShorelineAreaPos.y;
			//	return terrainDepth;
			//}

			float3 ComputeBeachWaveOffset(float2 uv, float2 prevUV, float alpha, float2x2 angleMatrix, float time, float3 scale)
			{
				float2 pos = KWS_ShorelineDisplacement.SampleLevel(sampler_linear_clamp, uv, 0).xy;
				float2 pos2 = KWS_ShorelineDisplacement.SampleLevel(sampler_linear_clamp, prevUV, 0).xy;
				pos = lerp(pos, pos2, time);
				
				float3 offsetWave = float3(pos.x, pos.x - 2, 0) * pos.y;
				offsetWave.xz = mul(angleMatrix, offsetWave.xz);
				offsetWave = offsetWave * alpha * scale * float3(0.25, 0.3, 0.25);
				return offsetWave;
			}


			inline float2 ComputeBeachWaveNormal(float2 uv, float2 prevUV, float uvAlpha, float texAlpha, float2x2 angleMatrix, float time)
			{
				float2 waveNorm = KWS_ShorelineNormal.Sample(sampler_linear_clamp, uv).xz;
				float2 waveNorm2 = KWS_ShorelineNormal.Sample(sampler_linear_clamp, prevUV).xz;

				waveNorm = lerp(waveNorm, waveNorm2, time);
				waveNorm = waveNorm * 2 - 1;
				waveNorm.x *= -1;

				waveNorm = mul(angleMatrix, waveNorm);
				return uvAlpha > 0.9 ? waveNorm * texAlpha : 0;
			}

			v2f vert(appdata v)
			{
				v2f o;
				ShorelineDataStruct data = KWS_ShorelineDataBuffer[v.instanceID];

				float sina, cosa;
				sincos(-data.angle * Deg2Rad, sina, cosa);
				float2x2 angleMatrix = float2x2(cosa, -sina, sina, cosa);

				v.vertex.xy = mul(angleMatrix, v.vertex.xy * data.size.xz * data.scale.xz);
				o.vertex = float4((data.position.xz + v.vertex.xy - KWS_ShorelineAreaPosSize.xz) / KWS_ShorelineAreaPosSize.w * 2, 0, 1);
				o.vertex.y = -o.vertex.y;
				o.uv = v.uv;
				o.instanceID = v.instanceID;
				//o.worldPos = float3(v.vertex.x + data.position.x, 0, v.vertex.y + data.position.z);
				return o;
			}

			FragmentOutput frag(v2f i)
			{
				FragmentOutput o;
				ShorelineDataStruct data = KWS_ShorelineDataBuffer[i.instanceID];
				
				float alpha = 0;
				if (i.uv.x > 0.03 && i.uv.x < 0.97 && i.uv.y > 0.03 && i.uv.y < 0.97) alpha = 1;
				
				i.uv.x = 1 - i.uv.x;

				KW_Time *= KW_GlobalTimeScale;
				KW_Time += data.timeOffset * KWS_SHORELINE_OFFSET_MULTIPLIER;
				float2 uv = GetAnimatedUV(i.uv, 14, 15, _FPS, KW_Time);
				float2 prevUV = GetAnimatedUV(i.uv, 14, 15, _FPS, KW_Time + 1.0 / _FPS);

				float sina, cosa;
				sincos(-data.angle * Deg2Rad, sina, cosa);
				float2x2 angleMatrix = float2x2(cosa, -sina, sina, cosa);
				float interpolationTime = frac(KW_Time * _FPS);

				float texAlpha = KWS_ShorelineAlpha.Sample(sampler_linear_clamp, uv).x;

				float3 offset = ComputeBeachWaveOffset(uv, prevUV, alpha, angleMatrix, interpolationTime, data.scale);
				float2 normal = ComputeBeachWaveNormal(uv, prevUV, alpha, texAlpha, angleMatrix, interpolationTime);

				//float terrainDepth = ComputeWaterOrthoDepth(i.worldPos);
				//float shorelineNearDepthMask = saturate(terrainDepth - KW_WaterPosition.y + 0.85);
				//offset.y = max(offset.y, max(0, offset.y) + terrainDepth - KW_WaterPosition.y - 0.05);

				o.dest0 = float4(offset, 1);
				o.dest1 = float4(normal, 1, 1);
				
				return o;
			}
			ENDHLSL
		}
	}
}