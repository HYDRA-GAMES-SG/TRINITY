Shader "Hidden/KriptoFX/KWS/FlowMapEdit" {
	Properties
	{
		_MainTex("Base (RGB)", 2D) = "" {}
	}

		HLSLINCLUDE

		#include "UnityCG.cginc"

		sampler2D _MainTex;
		float4 _MainTex_TexelSize;
		float2 _MousePos;
		half _Size;
		half2 _Direction;
		float _BrushStrength;
		int isErase;
		float _UvScale;

		struct appdata_t {
			float4 vertex : POSITION;
			float2 texcoord: TEXCOORD0;
		};

		struct v2f {
			float4 vertex : SV_POSITION;
			float2 uv : TEXCOORD0;
		};

		v2f vert(appdata_t v)
		{
			v2f o;
			o.vertex = UnityObjectToClipPos(v.vertex);
			o.uv = v.texcoord;
			return o;
		}

		float4 frag(v2f i) : SV_Target
		{
			float2 flowMap = tex2D(_MainTex, i.uv);
			if (i.uv.x <= 0.001 || i.uv.x >= 0.999 || i.uv.y <= 0.001 || i.uv.y >= 0.999) return float4(0.5, 0.5, 0, 0);
			
			float grad = length(_MousePos.xy - i.uv);
			grad = 1 - saturate(grad * _Size);
			
			float2 newVal;
			if (isErase == 1)
			{
				newVal = lerp(flowMap, float2(0.5, 0.5), grad * _BrushStrength);
			}

			else
			{
				newVal = flowMap + grad * _Direction * _BrushStrength;
				newVal = clamp(newVal, 0.05, 0.95);
			}

			return float4(newVal, 0, 0);
		}

		float4 fragResize(v2f i) : SV_Target
		{
			i.uv = (i.uv - 0.5) * _UvScale + 0.5;
			if (i.uv.x <= 0.001 || i.uv.x >= 0.999 || i.uv.y <= 0.001 || i.uv.y >= 0.999) return float4(0.5, 0.5, 0, 0);

			float2 resizedFlowMap = tex2D(_MainTex, i.uv).xy;

			return float4(resizedFlowMap, 0, 0);
		}

		ENDHLSL

		SubShader
		{
			Cull Off ZWrite Off ZTest Always

			Pass
			{
				HLSLPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				ENDHLSL
			}

			Pass
			{
				HLSLPROGRAM
				#pragma vertex vert
				#pragma fragment fragResize
				ENDHLSL
			}
	}
}
