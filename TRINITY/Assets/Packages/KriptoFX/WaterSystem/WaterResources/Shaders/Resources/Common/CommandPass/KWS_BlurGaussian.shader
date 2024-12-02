Shader "Hidden/KriptoFX/KWS/BlurGaussian"
{
	HLSLINCLUDE
	
	#include "../../PlatformSpecific/Includes/KWS_HelpersIncludes.cginc"

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

	DECLARE_TEXTURE(_SourceRT);
	float4 _SourceRT_TexelSize;
	float _SampleScale;
	float4 _SourceRTHandleScale;
	float _Threshold;

	vertexOutput vert(vertexInput v)
	{
		vertexOutput o;
		UNITY_SETUP_INSTANCE_ID(v);
		UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
		o.vertex = GetTriangleVertexPosition(v.vertexID);
		o.uv = GetTriangleUVScaled(v.vertexID);
		return o;
	}

	//inline float2 GetNormalizedUV(float2 uv)
	//{
	//	return clamp(uv, 0.01, 0.97) * _SourceRTHandleScale.xy;
	//}


	half4 frag_downsample(vertexOutput i) : SV_Target
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
		
		float4 d = _SourceRT_TexelSize.xyxy * float4(-1, -1, +1, +1);

		float2 uv = GetRTHandleUV(i.uv, _SourceRT_TexelSize, 2.0, _SourceRTHandleScale.xy);

		half4 s;
		s = SAMPLE_TEXTURE_LOD(_SourceRT, sampler_linear_clamp, (uv + d.xy), 0);
		s += SAMPLE_TEXTURE_LOD(_SourceRT, sampler_linear_clamp, (uv + d.zy), 0);
		s += SAMPLE_TEXTURE_LOD(_SourceRT, sampler_linear_clamp, (uv + d.xw), 0);
		s += SAMPLE_TEXTURE_LOD(_SourceRT, sampler_linear_clamp, (uv + d.zw), 0);

		return s * (1.0 / 4);
	}

	half4 frag_upsample(vertexOutput i) : SV_Target
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
		// 4-tap bilinear upsampler
		float4 d = _SourceRT_TexelSize.xyxy * float4(-1, -1, +1, +1) * (_SampleScale * 0.5);
		float2 uv = GetRTHandleUV(i.uv, _SourceRT_TexelSize, 2.0, _SourceRTHandleScale.xy);

		half4 s;
		s = SAMPLE_TEXTURE_LOD(_SourceRT, sampler_linear_clamp, (uv + d.xy), 0);
		s += SAMPLE_TEXTURE_LOD(_SourceRT, sampler_linear_clamp, (uv + d.zy), 0);
		s += SAMPLE_TEXTURE_LOD(_SourceRT, sampler_linear_clamp, (uv + d.xw), 0);
		s += SAMPLE_TEXTURE_LOD(_SourceRT, sampler_linear_clamp, (uv + d.zw), 0);

		return s * (1.0 / 4);
	}

	//1 4 6 4 1
	static const float gaussianWeight5[5] = 
	{
		//0.003,
		0.0625,
		0.25,
		0.375,
		0.25,
		0.0625
		//0.003,
	};

	//1 6 15 20 15 6 1
	static const float gaussianWeight7[7] = 
	{
		0.015627,//-3 0
		0.09375,//-2 1 
		0.234375,//-1 2
		0.312,//0 3
		0.234375,//1 4
		0.09375,//2 5
		0.015627//3 6
	};
	
	half4 frag_separable_low(float2 uv, float2 direction) : SV_Target
	{
		uv = GetRTHandleUVBilinear(uv, _SourceRT_TexelSize, 5.0, _SourceRTHandleScale.xy);
		float2 offset = direction * _SourceRT_TexelSize.xy * _SampleScale;
		half4 s;
		for (int idx = -2; idx <= 2; idx += 1)
		{
			float weight = gaussianWeight5[idx + 2];
			s += weight * SAMPLE_TEXTURE_LOD(_SourceRT, sampler_linear_clamp, uv + offset * idx, 0);
		}
		return s;
	}

	half4 frag_separable_high(float2 uv, float2 direction) : SV_Target
	{
		uv = GetRTHandleUVBilinear(uv, _SourceRT_TexelSize, 5.0, _SourceRTHandleScale.xy);
		float2 offset = direction * _SourceRT_TexelSize.xy * _SampleScale;
		half4 s;
		for (int idx = -3; idx <= 3; idx += 1)
		{
			float weight = gaussianWeight7[idx + 3];
			s += weight * SAMPLE_TEXTURE_LOD(_SourceRT, sampler_linear_clamp, uv + offset * idx, 0);
		}
		return s;
	}

	ENDHLSL

	SubShader
	{
		ZTest Always Cull Off ZWrite Off

		//0 downsample
		Pass
		{
			ZTest Always Cull Off ZWrite Off
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag_downsample
			ENDHLSL
		}
		//1 upsample
		Pass
		{
			ZTest Always Cull Off ZWrite Off
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag_upsample
			ENDHLSL
		}
		//2 horizontal low
		Pass
		{
			ZTest Always Cull Off ZWrite Off
			HLSLPROGRAM
			
			#pragma vertex vert
			#pragma fragment horizontalFrag
		

			half4 horizontalFrag(vertexOutput i) : SV_Target
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
				return frag_separable_low(i.uv, float2(1, 0));
			}
			ENDHLSL
		}
		//3 vertical low
		Pass
		{
			ZTest Always Cull Off ZWrite Off
			HLSLPROGRAM	
			#pragma vertex vert
			#pragma fragment verticalFrag
		

			half4 verticalFrag(vertexOutput i) : SV_Target
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
				return frag_separable_low(i.uv, float2(0, 1));
			}
			ENDHLSL
		}

		//4 horizontal high
		Pass
		{
			ZTest Always Cull Off ZWrite Off
			HLSLPROGRAM
			
			#pragma vertex vert
			#pragma fragment horizontalFrag
			

			half4 horizontalFrag(vertexOutput i) : SV_Target
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
				return frag_separable_high(i.uv, float2(1, 0));
			}
			ENDHLSL
		}

		//5 vertical high
		Pass
		{
			ZTest Always Cull Off ZWrite Off
			HLSLPROGRAM
			
			#pragma vertex vert
			#pragma fragment verticalFrag
		

			half4 verticalFrag(vertexOutput i) : SV_Target
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
				return frag_separable_high(i.uv, float2(0, 1));
			}
			ENDHLSL
		}
	}
}