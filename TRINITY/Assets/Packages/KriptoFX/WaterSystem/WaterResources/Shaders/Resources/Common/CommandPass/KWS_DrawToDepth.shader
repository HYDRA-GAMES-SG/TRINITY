Shader "Hidden/KriptoFX/KWS/DrawToDepth"
{

	SubShader
	{

		Cull Off ZWrite On ZTest Always

		Pass
		{
			HLSLPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma target 4.6

			#include "../../PlatformSpecific/Includes/KWS_HelpersIncludes.cginc"

			struct vertexInput
			{
				uint vertexID : SV_VertexID;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			v2f vert(vertexInput v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				o.vertex = GetTriangleVertexPosition(v.vertexID);
				o.uv = GetTriangleUVScaled(v.vertexID);
				return o;
			}

			DECLARE_TEXTURE(_SourceRT);
			float4 _SourceRTHandleScale;

			float4 frag(v2f i, out float depth : SV_Depth) : SV_Target
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
				half mask = GetWaterMask(i.uv).x;
				if (GetUnderwaterMask(mask))
				{
					depth = 0.99;
					return depth;
				}

				float sceneDepth = SAMPLE_TEXTURE_LOD(_SourceRT, sampler_linear_clamp, i.uv * _SourceRTHandleScale.xy, 0).x;
				float waterDepth = GetWaterDepth(i.uv - float2(0, KWS_WaterPrePassRT0_TexelSize.y * 0));
				depth = max(waterDepth, sceneDepth);
				return depth;
			}
			ENDHLSL
		}
	}
}