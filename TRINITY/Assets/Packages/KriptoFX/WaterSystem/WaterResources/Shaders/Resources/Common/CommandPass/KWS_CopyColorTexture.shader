Shader "Hidden/KriptoFX/KWS/CopyColorTexture"
{
    SubShader
    {
        Cull Off 
        ZWrite Off 
        ZTest Always

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "../../PlatformSpecific/Includes/KWS_HelpersIncludes.cginc"

            struct appdata_t 
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

            v2f vert(appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = GetTriangleVertexPosition(v.vertexID);
                o.uv = GetTriangleUVScaled(v.vertexID);
                return o;
            }
            
            DECLARE_TEXTURE(_SourceRT);

            float4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                return SAMPLE_TEXTURE_LOD(_SourceRT, sampler_linear_clamp, i.uv, 0);
            }
            ENDHLSL
        }
    }
}
