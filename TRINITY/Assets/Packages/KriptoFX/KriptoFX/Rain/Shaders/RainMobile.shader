Shader "Effects/WaterDropMobile"
{
	Properties
	{
		[HDR]_TintColor ("Tint Color", Color) = (1,1,1,1)
		_MainTex ("Texture", 2D) = "white" {}
		_BumpMap ("Normalmap", 2D) = "bump" {}
		_BumpAmt ("Distortion", Float) = 10
	}
	SubShader
	{
		Tags { "Queue"="Transparent"  "IgnoreProjector"="True"  "RenderType"="Transparent" }
		Blend SrcAlpha One
		Cull Off 
		Lighting Off 
		ZWrite Off 
		
		Offset -1, -1

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				fixed4 color : COLOR;
			};

			struct v2f
			{
				float4 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				fixed4 color : COLOR;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			half4 _TintColor;
			sampler2D _BumpMap;
			float4 _BumpMap_ST;
			half _BumpAmt;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.color = v.color;
				o.uv.xy = TRANSFORM_TEX(v.uv, _MainTex);
				o.uv.zw = TRANSFORM_TEX(v.uv, _BumpMap);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				half2 bump = UnpackNormal(tex2D( _BumpMap, i.uv.zw )).rg;
				i.uv.xy += bump * _BumpAmt * i.color.r;
				fixed4 col = tex2D(_MainTex, i.uv.xy) * _TintColor * i.color.a;
				return col;
			}
			ENDCG
		}
	}
}
