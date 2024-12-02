Shader "Hidden/KriptoFX/KWS/CopyDepthTexture"
{
	SubShader
	{
		//Cull Off
		//ZWrite Off
		//ZTest Always

		Pass
		{
			HLSLPROGRAM

			#pragma vertex vert
			#pragma fragment frag

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

			float4 frag(v2f i, out float depth : SV_Depth) : SV_Target
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
				depth = GetSceneDepth(i.uv);
				return float4(depth, 0, 0, 1);
			}
			ENDHLSL
		}
	}
}