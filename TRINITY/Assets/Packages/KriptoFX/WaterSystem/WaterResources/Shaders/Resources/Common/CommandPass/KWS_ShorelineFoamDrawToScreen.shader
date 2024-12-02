Shader "Hidden/KriptoFX/KWS/ShorelineFoamDrawToScreen"
{
	HLSLINCLUDE

	#pragma multi_compile _ KWS_FOAM_USE_FAST_MODE

	#include "../../PlatformSpecific/Includes/KWS_HelpersIncludes.cginc"

	#pragma vertex vert
	#pragma fragment frag
	
	#if defined(KWS_FOAM_USE_FAST_MODE)
		DECLARE_TEXTURE_UINT(_SourceRT);
	#else
		DECLARE_TEXTURE(_SourceRT);
	#endif

	float4 _FoamRTSize;
	float4 KWS_ShorelineColor;

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

	ENDHLSL

	SubShader
	{
		Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		

		//write foam color to colorRT
		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha
			ZTest Always
			ZWrite Off

			HLSLPROGRAM

			half4 Unpack_R8G8B8A8n(uint data)
			{
				return half4(
					(data >> 24) & 0xFF,
					(data >> 16) & 0xFF,
					(data >> 8) & 0xFF,
					data & 0xFF) / 255.0;
			}

			vertexOutput vert(vertexInput v)
			{
				vertexOutput o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				o.vertex = GetTriangleVertexPosition(v.vertexID);
				o.uv = GetTriangleUVScaled(v.vertexID);
				return o;
			}

			half4 frag(vertexOutput i) : SV_Target
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

				float alphaScale = 0.9;
				float colorScale = 0.85;
				#if defined(KWS_FOAM_USE_FAST_MODE)
					uint rawData = LOAD_TEXTURE(_SourceRT, uint2(i.uv * _FoamRTSize.xy)).r;
					float4 foam = Unpack_R8G8B8A8n(rawData); //sorting by alpha in first 8 bits
					return foam.yzwx * KWS_ShorelineColor;
				#else
					half4 foam = SAMPLE_TEXTURE_LOD(_SourceRT, sampler_linear_clamp, i.uv, 0);
					return foam * KWS_ShorelineColor;
				#endif
			}

			ENDHLSL
		}
	}
}