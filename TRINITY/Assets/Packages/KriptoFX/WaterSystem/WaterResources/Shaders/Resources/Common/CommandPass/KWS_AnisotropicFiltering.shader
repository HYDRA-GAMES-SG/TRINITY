Shader "Hidden/KriptoFX/KWS/AnisotropicFiltering"
{

	HLSLINCLUDE
	//#pragma multi_compile _ USE_STEREO_ARRAY

	#include "../../PlatformSpecific/Includes/KWS_HelpersIncludes.cginc"

	#define ANISO_MAX_DISTANCE 1.0/5.0
	#define ANISO_MIN_DISTANCE 1
	#define ANISO_MUL 0.125

	float KWS_IsCubemapSide;

	//#ifdef USE_STEREO_ARRAY
		DECLARE_TEXTURE(_SourceRT);
	//#else
	//	Texture2D _SourceRT;
	//#endif
	
	float4 _SourceRT_TexelSize;
	float4 _SourceRTHandleScale;

	uint KWS_ReverseBits32(uint bits)
	{
		#if 0 // Shader model 5
			return reversebits(bits);
		#else
			bits = (bits << 16) | (bits >> 16);
			bits = ((bits & 0x00ff00ff) << 8) | ((bits & 0xff00ff00) >> 8);
			bits = ((bits & 0x0f0f0f0f) << 4) | ((bits & 0xf0f0f0f0) >> 4);
			bits = ((bits & 0x33333333) << 2) | ((bits & 0xcccccccc) >> 2);
			bits = ((bits & 0x55555555) << 1) | ((bits & 0xaaaaaaaa) >> 1);
			return bits;
		#endif
	}
	//-----------------------------------------------------------------------------
	float KWS_RadicalInverse_VdC(uint bits)
	{
		return float(KWS_ReverseBits32(bits)) * 2.3283064365386963e-10; // 0x100000000

	}

	//-----------------------------------------------------------------------------
	float2 KWS_Hammersley2d(uint i, uint maxSampleCount)
	{
		return float2(float(i) / float(maxSampleCount), KWS_RadicalInverse_VdC(i));
	}

	//-----------------------------------------------------------------------------
	float KWS_HashRand(uint s)
	{
		s = s ^ 2747636419u;
		s = s * 2654435769u;
		s = s ^(s >> 16);
		s = s * 2654435769u;
		s = s ^(s >> 16);
		s = s * 2654435769u;
		return float(s) / 4294967295.0f;
	}

	//-----------------------------------------------------------------------------
	float KWS_InitRand(float input)
	{
		return KWS_HashRand(uint(input * 4294967295.0f));
	}


	float GetAnisoScale(float2 uv)
	{
		//return Test4.x;
		UNITY_BRANCH
		if (KWS_WaterInstancesCount == 1) return GetAnisoScaleRelativeToWind(KWS_WindSpeedArray[1]);
		else
		{
			uint waterID = GetWaterID(uv);
			return GetAnisoScaleRelativeToWind(KWS_WindSpeedArray[waterID]);
		}
	}

	half4 ReflectionPreFiltering(float2 uv, const uint SAMPLE_COUNT)
	{
		half4 prefilteredColor = 0.0;
		float randNum = KWS_InitRand(uv.x * uv.y);

		float depth = GetWaterDepth(uv);
		float3 worldPos = GetWorldSpacePositionFromDepth(uv, depth);
		float3 camPos = GetCameraAbsolutePosition();
		float distance = length(worldPos.xz - camPos.xz);
		float anisoScaleRelativeToDistance = saturate((distance - ANISO_MIN_DISTANCE) * ANISO_MAX_DISTANCE);
		
		float anisoScale = GetAnisoScale(uv) * ANISO_MUL * anisoScaleRelativeToDistance;

		//half underwaterMask = GetWaterMask(uv);
		//if(underwaterMask > 0.99) anisoScale += 0.035;
		//float4 defaultColor = GetSceneColor(uv);
		uv *= _SourceRTHandleScale.xy;
		
		UNITY_LOOP
		for (uint i = 0u; i < SAMPLE_COUNT; ++i)
		{
			float2 filterUV =  uv + float2(0, (1.0 * i) / SAMPLE_COUNT - 0.25 + randNum * 0.1) * anisoScale;
			filterUV = clamp(filterUV, 0.001, 0.999);
		
			//#ifdef USE_STEREO_ARRAY
				float4 color = max(0, SAMPLE_TEXTURE_LOD(_SourceRT, sampler_linear_repeat, filterUV, 0));
			//#else
			//	float4 color = max(0, _SourceRT.SampleLevel(sampler_linear_repeat, filterUV, 0));
			//#endif
		
			prefilteredColor += color;
		
		}
		prefilteredColor = prefilteredColor / (1.0 * SAMPLE_COUNT);
	
		return prefilteredColor;
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

	vertexOutput vert(vertexInput v)
	{
		vertexOutput o;
		UNITY_SETUP_INSTANCE_ID(v);
		UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
		o.vertex = GetTriangleVertexPosition(v.vertexID);
		o.uv = GetTriangleUVScaled(v.vertexID) ;
		return o;
	}


	ENDHLSL


	SubShader
	{
		Pass //low quality

		{
			HLSLPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma target 4.6

			#define SAMPLE_COUNT 7

			half4 frag(vertexOutput i) : SV_Target
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
				return ReflectionPreFiltering(i.uv, SAMPLE_COUNT);
			}

			ENDHLSL
		}


		Pass //high quality

		{
			HLSLPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma target 4.6

			#define SAMPLE_COUNT 13

			half4 frag(vertexOutput i) : SV_Target
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
				return ReflectionPreFiltering(i.uv, SAMPLE_COUNT);
			}

			ENDHLSL
		}
	}
}