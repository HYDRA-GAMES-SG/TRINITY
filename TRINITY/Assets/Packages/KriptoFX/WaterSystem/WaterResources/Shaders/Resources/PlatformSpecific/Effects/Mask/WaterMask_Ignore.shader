Shader "KriptoFX/KWS/WaterMask_Ignore"
{
	Properties
	{
		[HideInInspector]KWS_StencilMaskValue("KWS_StencilMaskValue", Int) = 32
	}

	SubShader
	{
		Tags { "Queue" = "Geometry+1" }
		ColorMask 0
		ZWrite Off
		//ZTest Always
		Cull Off

		Stencil
		{
			Ref [KWS_StencilMaskValue]
			WriteMask [KWS_StencilMaskValue]
			Comp always
			Pass replace
		}


		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				return o;
			}

			half4 frag(v2f i) : SV_Target
			{
				return 0;
			}
			ENDCG
		}
	}
}