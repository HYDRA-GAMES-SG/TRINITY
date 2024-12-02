Shader "Hidden/KriptoFX/KWS/Water/Debug/TextureSlice"
{
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass //slice
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            int _slice;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            Texture2DArray _MainTex;
            SamplerState sampler_linear_repeat;

            float4 frag (v2f i) : SV_Target
            {
                return _MainTex.SampleLevel(sampler_linear_repeat, float3(i.uv, _slice), 0);
            }
            ENDCG
        }

        Pass //normal
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            int _slice;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            Texture2DArray _MainTex;
            SamplerState sampler_linear_repeat;

            float4 frag (v2f i) : SV_Target
            {
                return _MainTex.SampleLevel(sampler_linear_repeat, float3(i.uv, _slice), 0).y;
            }
            ENDCG
        }
    }
}
