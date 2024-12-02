Shader "Hidden/KriptoFX/KWS/UnderwaterBlurToScreenPass"
{
	Properties
	{
		[HideInInspector]KWS_StencilMaskValue ("KWS_StencilMaskValue", Int) = 32
	}


	SubShader
	{
		Tags { "Queue" = "Transparent+1" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		ZTest Always
		ZWrite Off

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
			#pragma vertex vertBlurToScreen
			#pragma fragment fragBlurToScreen
			#pragma target 4.6

			#include "../../PlatformSpecific/Includes/KWS_HelpersIncludes.cginc"
			
			DECLARE_TEXTURE(_SourceRT);
			float4 _SourceRTHandleScale;

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


			vertexOutput vertBlurToScreen(vertexInput v)
			{
				vertexOutput o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				o.vertex = GetTriangleVertexPosition(v.vertexID);
				o.uv = GetTriangleUVScaled(v.vertexID);
				return o;
			}


			half4 fragBlurToScreen(vertexOutput i) : SV_Target
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
				half mask = GetWaterMask(i.uv);
				if (mask < 0.01) return 0;

				half3 underwaterColor = SAMPLE_TEXTURE_LOD(_SourceRT, sampler_linear_clamp, i.uv * _SourceRTHandleScale.xy, 0).xyz;
				return half4(underwaterColor, MaskToAlpha(mask));
			}


			ENDHLSL
		}
	}
}