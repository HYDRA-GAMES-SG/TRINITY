Shader "Hidden/KriptoFX/KWS/KWS_BuoyancyPass_FftToHeight"
{
	SubShader
	{
		Pass
		{
			//Blend One One

			ZWrite Off
			Cull Front

			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			
			#include "../../PlatformSpecific/Includes/KWS_HelpersIncludes.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float height : TEXCOORD0;
			};


			v2f vert(appdata v)
			{
				v2f o;

				float3 worldPos = 0;

				float domainSize = GetDomainSize(KWS_WavesCascades - 1);
				worldPos.xz = float2(1, -1) * v.vertex.xy;
				float3 offset = GetFftWavesDisplacementBuoyancy((worldPos * domainSize));
				v.vertex.xy += (float2(1, -1) * offset.xz / domainSize);
				o.height = offset.y;
				o.vertex = float4(v.vertex.xy, 0, 0.5);
				return o;
			}

			float4 frag(v2f i) : SV_Target
			{
				return float4(i.height, 1, 0, 1);
			}
			
			ENDHLSL
		}
	}
}