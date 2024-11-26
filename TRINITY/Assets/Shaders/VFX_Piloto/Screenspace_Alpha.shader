// Made with Amplify Shader Editor v1.9.3.3
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Piloto Studio/Screenspace Alpha"
{
	Properties
	{
		[HideInInspector] _AlphaCutoff("Alpha Cutoff ", Range(0, 1)) = 0.5
		[HideInInspector] _EmissionColor("Emission Color", Color) = (1,1,1,1)
		_MainTex("Main Texture", 2D) = "white" {}
		_MainTextureChannel("Main Texture Channel", Vector) = (1,1,1,0)
		_MainTexturePanning("Main Texture Panning", Vector) = (0,0,0,0)
		_AlphaOverride("Alpha Override", 2D) = "white" {}
		_AlphaOverridePanning("Alpha Override Panning", Vector) = (0,0,0,0)
		_AlphaOverrideChannel("Alpha Override Channel", Vector) = (1,0,0,0)
		_DetailNoise("Detail Noise", 2D) = "white" {}
		_DetailNoisePanning("Detail Noise Panning", Vector) = (0,0,0,0)
		_DetailDistortionChannel("Detail Distortion Channel", Vector) = (1,0,0,0)
		_DistortionStrenght("Distortion Strenght", Float) = 0
		_DetailMultiplyChannel("Detail Multiply Channel", Vector) = (0,0,0,0)
		_DetailAdditiveChannel("Detail Additive Channel", Vector) = (0,0,0,0)
		_Desaturate("Desaturate?", Range( 0 , 1)) = 0

		[HideInInspector][NoScaleOffset] unity_Lightmaps("unity_Lightmaps", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset] unity_LightmapsInd("unity_LightmapsInd", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset] unity_ShadowMasks("unity_ShadowMasks", 2DArray) = "" {}
	}

	SubShader
	{
		LOD 0

		

		Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent" }

		Cull Off
		HLSLINCLUDE
		#pragma target 2.0
		#pragma prefer_hlslcc gles
		// ensure rendering platforms toggle list is visible

		#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
		#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Filtering.hlsl"
		ENDHLSL

		
		Pass
		{
			Name "Sprite Unlit"
			Tags { "LightMode"="Universal2D" }

			Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
			ZTest LEqual
			ZWrite Off
			Offset 0 , 0
			ColorMask RGBA
			

			HLSLPROGRAM

			#define ASE_SRP_VERSION 140010


			#pragma vertex vert
			#pragma fragment frag

			#define _SURFACE_TYPE_TRANSPARENT 1
			#define SHADERPASS SHADERPASS_SPRITEUNLIT

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			#include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/SurfaceData2D.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Debug/Debugging2D.hlsl"

			#define ASE_NEEDS_FRAG_COLOR


			sampler2D _MainTex;
			sampler2D _DetailNoise;
			sampler2D _AlphaOverride;
			CBUFFER_START( UnityPerMaterial )
			float4 _DetailDistortionChannel;
			float4 _MainTextureChannel;
			float4 _DetailMultiplyChannel;
			float4 _DetailAdditiveChannel;
			float4 _AlphaOverrideChannel;
			float2 _MainTexturePanning;
			float2 _DetailNoisePanning;
			float2 _AlphaOverridePanning;
			float _DistortionStrenght;
			float _Desaturate;
			CBUFFER_END


			struct VertexInput
			{
				float4 positionOS : POSITION;
				float3 normal : NORMAL;
				float4 tangent : TANGENT;
				float4 uv0 : TEXCOORD0;
				float4 color : COLOR;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 positionCS : SV_POSITION;
				float4 texCoord0 : TEXCOORD0;
				float4 color : TEXCOORD1;
				float3 positionWS : TEXCOORD2;
				float4 ase_texcoord3 : TEXCOORD3;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			#if ETC1_EXTERNAL_ALPHA
				TEXTURE2D( _AlphaTex ); SAMPLER( sampler_AlphaTex );
				float _EnableAlphaTexture;
			#endif

			float4 _RendererColor;

			
			VertexOutput vert( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );

				float4 ase_clipPos = TransformObjectToHClip((v.positionOS).xyz);
				float4 screenPos = ComputeScreenPos(ase_clipPos);
				o.ase_texcoord3 = screenPos;
				
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3( 0, 0, 0 );
				#endif
				float3 vertexValue = defaultVertexValue;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.positionOS.xyz = vertexValue;
				#else
					v.positionOS.xyz += vertexValue;
				#endif
				v.normal = v.normal;
				v.tangent.xyz = v.tangent.xyz;

				VertexPositionInputs vertexInput = GetVertexPositionInputs( v.positionOS.xyz );

				o.texCoord0 = v.uv0;
				o.color = v.color;
				o.positionCS = vertexInput.positionCS;
				o.positionWS = vertexInput.positionWS;

				return o;
			}

			half4 frag( VertexOutput IN  ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				float2 texCoord81 = IN.texCoord0.xy * float2( 1,1 ) + float2( 0,0 );
				float2 panner80 = ( 1.0 * _Time.y * _DetailNoisePanning + texCoord81);
				float4 tex2DNode79 = tex2D( _DetailNoise, panner80 );
				float4 break17_g63 = tex2DNode79;
				float4 appendResult18_g63 = (float4(break17_g63.x , break17_g63.y , break17_g63.z , break17_g63.w));
				float4 clampResult19_g63 = clamp( ( appendResult18_g63 * _DetailDistortionChannel ) , float4( 0,0,0,0 ) , float4( 1,1,1,1 ) );
				float4 break2_g63 = clampResult19_g63;
				float clampResult20_g63 = clamp( ( break2_g63.x + break2_g63.y + break2_g63.z + break2_g63.w ) , 0.0 , 1.0 );
				float DistortionNoise90 = clampResult20_g63;
				float4 screenPos = IN.ase_texcoord3;
				float4 ase_screenPosNorm = screenPos / screenPos.w;
				ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
				float2 panner22 = ( 1.0 * _Time.y * _MainTexturePanning + ( ( DistortionNoise90 * _DistortionStrenght ) + ( (float2( 0,0 ) + ((ase_screenPosNorm).xy - float2( 0,0 )) * (float2( 1,1 ) - float2( 0,0 )) / (float2( 1,1 ) - float2( 0,0 ))) * float2( 4,4 ) ) ));
				float4 tex2DNode150 = tex2D( _MainTex, panner22 );
				float4 break17_g67 = tex2DNode150;
				float4 appendResult18_g67 = (float4(break17_g67.x , break17_g67.y , break17_g67.z , break17_g67.w));
				float4 clampResult19_g67 = clamp( ( appendResult18_g67 * _MainTextureChannel ) , float4( 0,0,0,0 ) , float4( 1,1,1,1 ) );
				float4 break2_g67 = clampResult19_g67;
				float clampResult20_g67 = clamp( ( break2_g67.x + break2_g67.y + break2_g67.z + break2_g67.w ) , 0.0 , 1.0 );
				float3 temp_cast_2 = (clampResult20_g67).xxx;
				float3 desaturateInitialColor158 = temp_cast_2;
				float desaturateDot158 = dot( desaturateInitialColor158, float3( 0.299, 0.587, 0.114 ));
				float3 desaturateVar158 = lerp( desaturateInitialColor158, desaturateDot158.xxx, _Desaturate );
				float4 texCoord162 = IN.texCoord0;
				texCoord162.xy = IN.texCoord0.xy * float2( 1,1 ) + float2( 0,0 );
				float4 break17_g65 = tex2DNode79;
				float4 appendResult18_g65 = (float4(break17_g65.x , break17_g65.y , break17_g65.z , break17_g65.w));
				float4 clampResult19_g65 = clamp( ( appendResult18_g65 * _DetailMultiplyChannel ) , float4( 0,0,0,0 ) , float4( 1,1,1,1 ) );
				float4 break2_g65 = clampResult19_g65;
				float clampResult20_g65 = clamp( ( break2_g65.x + break2_g65.y + break2_g65.z + break2_g65.w ) , 0.0 , 1.0 );
				float ifLocalVar106 = 0;
				if( ( _DetailMultiplyChannel.x + _DetailMultiplyChannel.y + _DetailMultiplyChannel.z + _DetailMultiplyChannel.w ) <= 0.0 )
				ifLocalVar106 = 1.0;
				else
				ifLocalVar106 = clampResult20_g65;
				float MultiplyNoise92 = ifLocalVar106;
				float4 break17_g66 = tex2DNode79;
				float4 appendResult18_g66 = (float4(break17_g66.x , break17_g66.y , break17_g66.z , break17_g66.w));
				float4 clampResult19_g66 = clamp( ( appendResult18_g66 * _DetailAdditiveChannel ) , float4( 0,0,0,0 ) , float4( 1,1,1,1 ) );
				float4 break2_g66 = clampResult19_g66;
				float clampResult20_g66 = clamp( ( break2_g66.x + break2_g66.y + break2_g66.z + break2_g66.w ) , 0.0 , 1.0 );
				float AdditiveNoise91 = clampResult20_g66;
				float4 break180 = ( ( IN.color * float4( desaturateVar158 , 0.0 ) * ( texCoord162.z + 1.0 ) * MultiplyNoise92 ) + AdditiveNoise91 );
				float4 texCoord60 = IN.texCoord0;
				texCoord60.xy = IN.texCoord0.xy * float2( 1,1 ) + float2( 0,0 );
				float2 texCoord43 = IN.texCoord0.xy * float2( 1,1 ) + float2( 0,0 );
				float2 panner44 = ( 1.0 * _Time.y * _AlphaOverridePanning + texCoord43);
				float4 tex2DNode45 = tex2D( _AlphaOverride, panner44 );
				float4 break2_g64 = ( tex2DNode45 * _AlphaOverrideChannel );
				float AlphaOverride49 = saturate( ( break2_g64.x + break2_g64.y + break2_g64.z + break2_g64.w ) );
				float temp_output_3_0_g68 = ( texCoord60.w - ( 1.0 - AlphaOverride49 ) );
				float4 appendResult181 = (float4(break180.r , break180.g , break180.b , ( IN.color.a * saturate( saturate( ( temp_output_3_0_g68 / fwidth( temp_output_3_0_g68 ) ) ) ) * AlphaOverride49 )));
				
				float4 Color = appendResult181;

				#if ETC1_EXTERNAL_ALPHA
					float4 alpha = SAMPLE_TEXTURE2D( _AlphaTex, sampler_AlphaTex, IN.texCoord0.xy );
					Color.a = lerp( Color.a, alpha.r, _EnableAlphaTexture );
				#endif

				#if defined(DEBUG_DISPLAY)
				SurfaceData2D surfaceData;
				InitializeSurfaceData(Color.rgb, Color.a, surfaceData);
				InputData2D inputData;
				InitializeInputData(IN.positionWS.xy, half2(IN.texCoord0.xy), inputData);
				half4 debugColor = 0;

				SETUP_DEBUG_DATA_2D(inputData, IN.positionWS);

				if (CanDebugOverrideOutputColor(surfaceData, inputData, debugColor))
				{
					return debugColor;
				}
				#endif

				Color *= IN.color * _RendererColor;
				return Color;
			}

			ENDHLSL
		}
		
		Pass
		{
			
			Name "Sprite Unlit Forward"
            Tags { "LightMode"="UniversalForward" }

			Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
			ZTest LEqual
			ZWrite Off
			Offset 0 , 0
			ColorMask RGBA
			

			HLSLPROGRAM

			#define ASE_SRP_VERSION 140010


			#pragma vertex vert
			#pragma fragment frag

			#define _SURFACE_TYPE_TRANSPARENT 1
			#define SHADERPASS SHADERPASS_SPRITEFORWARD

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			#include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/SurfaceData2D.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Debug/Debugging2D.hlsl"

			#define ASE_NEEDS_FRAG_COLOR


			sampler2D _MainTex;
			sampler2D _DetailNoise;
			sampler2D _AlphaOverride;
			CBUFFER_START( UnityPerMaterial )
			float4 _DetailDistortionChannel;
			float4 _MainTextureChannel;
			float4 _DetailMultiplyChannel;
			float4 _DetailAdditiveChannel;
			float4 _AlphaOverrideChannel;
			float2 _MainTexturePanning;
			float2 _DetailNoisePanning;
			float2 _AlphaOverridePanning;
			float _DistortionStrenght;
			float _Desaturate;
			CBUFFER_END


			struct VertexInput
			{
				float4 positionOS : POSITION;
				float3 normal : NORMAL;
				float4 tangent : TANGENT;
				float4 uv0 : TEXCOORD0;
				float4 color : COLOR;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 positionCS : SV_POSITION;
				float4 texCoord0 : TEXCOORD0;
				float4 color : TEXCOORD1;
				float3 positionWS : TEXCOORD2;
				float4 ase_texcoord3 : TEXCOORD3;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			#if ETC1_EXTERNAL_ALPHA
				TEXTURE2D( _AlphaTex ); SAMPLER( sampler_AlphaTex );
				float _EnableAlphaTexture;
			#endif

			float4 _RendererColor;

			
			VertexOutput vert( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );

				float4 ase_clipPos = TransformObjectToHClip((v.positionOS).xyz);
				float4 screenPos = ComputeScreenPos(ase_clipPos);
				o.ase_texcoord3 = screenPos;
				
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3( 0, 0, 0 );
				#endif
				float3 vertexValue = defaultVertexValue;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.positionOS.xyz = vertexValue;
				#else
					v.positionOS.xyz += vertexValue;
				#endif
				v.normal = v.normal;
				v.tangent.xyz = v.tangent.xyz;

				VertexPositionInputs vertexInput = GetVertexPositionInputs( v.positionOS.xyz );

				o.texCoord0 = v.uv0;
				o.color = v.color;
				o.positionCS = vertexInput.positionCS;
				o.positionWS = vertexInput.positionWS;

				return o;
			}

			half4 frag( VertexOutput IN  ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				float2 texCoord81 = IN.texCoord0.xy * float2( 1,1 ) + float2( 0,0 );
				float2 panner80 = ( 1.0 * _Time.y * _DetailNoisePanning + texCoord81);
				float4 tex2DNode79 = tex2D( _DetailNoise, panner80 );
				float4 break17_g63 = tex2DNode79;
				float4 appendResult18_g63 = (float4(break17_g63.x , break17_g63.y , break17_g63.z , break17_g63.w));
				float4 clampResult19_g63 = clamp( ( appendResult18_g63 * _DetailDistortionChannel ) , float4( 0,0,0,0 ) , float4( 1,1,1,1 ) );
				float4 break2_g63 = clampResult19_g63;
				float clampResult20_g63 = clamp( ( break2_g63.x + break2_g63.y + break2_g63.z + break2_g63.w ) , 0.0 , 1.0 );
				float DistortionNoise90 = clampResult20_g63;
				float4 screenPos = IN.ase_texcoord3;
				float4 ase_screenPosNorm = screenPos / screenPos.w;
				ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
				float2 panner22 = ( 1.0 * _Time.y * _MainTexturePanning + ( ( DistortionNoise90 * _DistortionStrenght ) + ( (float2( 0,0 ) + ((ase_screenPosNorm).xy - float2( 0,0 )) * (float2( 1,1 ) - float2( 0,0 )) / (float2( 1,1 ) - float2( 0,0 ))) * float2( 4,4 ) ) ));
				float4 tex2DNode150 = tex2D( _MainTex, panner22 );
				float4 break17_g67 = tex2DNode150;
				float4 appendResult18_g67 = (float4(break17_g67.x , break17_g67.y , break17_g67.z , break17_g67.w));
				float4 clampResult19_g67 = clamp( ( appendResult18_g67 * _MainTextureChannel ) , float4( 0,0,0,0 ) , float4( 1,1,1,1 ) );
				float4 break2_g67 = clampResult19_g67;
				float clampResult20_g67 = clamp( ( break2_g67.x + break2_g67.y + break2_g67.z + break2_g67.w ) , 0.0 , 1.0 );
				float3 temp_cast_2 = (clampResult20_g67).xxx;
				float3 desaturateInitialColor158 = temp_cast_2;
				float desaturateDot158 = dot( desaturateInitialColor158, float3( 0.299, 0.587, 0.114 ));
				float3 desaturateVar158 = lerp( desaturateInitialColor158, desaturateDot158.xxx, _Desaturate );
				float4 texCoord162 = IN.texCoord0;
				texCoord162.xy = IN.texCoord0.xy * float2( 1,1 ) + float2( 0,0 );
				float4 break17_g65 = tex2DNode79;
				float4 appendResult18_g65 = (float4(break17_g65.x , break17_g65.y , break17_g65.z , break17_g65.w));
				float4 clampResult19_g65 = clamp( ( appendResult18_g65 * _DetailMultiplyChannel ) , float4( 0,0,0,0 ) , float4( 1,1,1,1 ) );
				float4 break2_g65 = clampResult19_g65;
				float clampResult20_g65 = clamp( ( break2_g65.x + break2_g65.y + break2_g65.z + break2_g65.w ) , 0.0 , 1.0 );
				float ifLocalVar106 = 0;
				if( ( _DetailMultiplyChannel.x + _DetailMultiplyChannel.y + _DetailMultiplyChannel.z + _DetailMultiplyChannel.w ) <= 0.0 )
				ifLocalVar106 = 1.0;
				else
				ifLocalVar106 = clampResult20_g65;
				float MultiplyNoise92 = ifLocalVar106;
				float4 break17_g66 = tex2DNode79;
				float4 appendResult18_g66 = (float4(break17_g66.x , break17_g66.y , break17_g66.z , break17_g66.w));
				float4 clampResult19_g66 = clamp( ( appendResult18_g66 * _DetailAdditiveChannel ) , float4( 0,0,0,0 ) , float4( 1,1,1,1 ) );
				float4 break2_g66 = clampResult19_g66;
				float clampResult20_g66 = clamp( ( break2_g66.x + break2_g66.y + break2_g66.z + break2_g66.w ) , 0.0 , 1.0 );
				float AdditiveNoise91 = clampResult20_g66;
				float4 break180 = ( ( IN.color * float4( desaturateVar158 , 0.0 ) * ( texCoord162.z + 1.0 ) * MultiplyNoise92 ) + AdditiveNoise91 );
				float4 texCoord60 = IN.texCoord0;
				texCoord60.xy = IN.texCoord0.xy * float2( 1,1 ) + float2( 0,0 );
				float2 texCoord43 = IN.texCoord0.xy * float2( 1,1 ) + float2( 0,0 );
				float2 panner44 = ( 1.0 * _Time.y * _AlphaOverridePanning + texCoord43);
				float4 tex2DNode45 = tex2D( _AlphaOverride, panner44 );
				float4 break2_g64 = ( tex2DNode45 * _AlphaOverrideChannel );
				float AlphaOverride49 = saturate( ( break2_g64.x + break2_g64.y + break2_g64.z + break2_g64.w ) );
				float temp_output_3_0_g68 = ( texCoord60.w - ( 1.0 - AlphaOverride49 ) );
				float4 appendResult181 = (float4(break180.r , break180.g , break180.b , ( IN.color.a * saturate( saturate( ( temp_output_3_0_g68 / fwidth( temp_output_3_0_g68 ) ) ) ) * AlphaOverride49 )));
				
				float4 Color = appendResult181;

				#if ETC1_EXTERNAL_ALPHA
					float4 alpha = SAMPLE_TEXTURE2D( _AlphaTex, sampler_AlphaTex, IN.texCoord0.xy );
					Color.a = lerp( Color.a, alpha.r, _EnableAlphaTexture );
				#endif


				#if defined(DEBUG_DISPLAY)
				SurfaceData2D surfaceData;
				InitializeSurfaceData(Color.rgb, Color.a, surfaceData);
				InputData2D inputData;
				InitializeInputData(IN.positionWS.xy, half2(IN.texCoord0.xy), inputData);
				half4 debugColor = 0;

				SETUP_DEBUG_DATA_2D(inputData, IN.positionWS);

				if (CanDebugOverrideOutputColor(surfaceData, inputData, debugColor))
				{
					return debugColor;
				}
				#endif

				Color *= IN.color * _RendererColor;
				return Color;
			}

			ENDHLSL
		}
		
        Pass
        {
			
            Name "SceneSelectionPass"
            Tags { "LightMode"="SceneSelectionPass" }

            Cull Off

            HLSLPROGRAM

			#define ASE_SRP_VERSION 140010


			#pragma vertex vert
			#pragma fragment frag

            #define _SURFACE_TYPE_TRANSPARENT 1
            #define ATTRIBUTES_NEED_NORMAL
            #define ATTRIBUTES_NEED_TANGENT
            #define FEATURES_GRAPH_VERTEX
            #define SHADERPASS SHADERPASS_DEPTHONLY
			#define SCENESELECTIONPASS 1


            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			#define ASE_NEEDS_FRAG_COLOR


			sampler2D _MainTex;
			sampler2D _DetailNoise;
			sampler2D _AlphaOverride;
			CBUFFER_START( UnityPerMaterial )
			float4 _DetailDistortionChannel;
			float4 _MainTextureChannel;
			float4 _DetailMultiplyChannel;
			float4 _DetailAdditiveChannel;
			float4 _AlphaOverrideChannel;
			float2 _MainTexturePanning;
			float2 _DetailNoisePanning;
			float2 _AlphaOverridePanning;
			float _DistortionStrenght;
			float _Desaturate;
			CBUFFER_END


            struct VertexInput
			{
				float3 positionOS : POSITION;
				float3 normal : NORMAL;
				float4 tangent : TANGENT;
				float4 ase_color : COLOR;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 positionCS : SV_POSITION;
				float4 ase_color : COLOR;
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_texcoord1 : TEXCOORD1;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};


            int _ObjectId;
            int _PassValue;

			
			VertexOutput vert(VertexInput v )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );

				float4 ase_clipPos = TransformObjectToHClip((v.positionOS).xyz);
				float4 screenPos = ComputeScreenPos(ase_clipPos);
				o.ase_texcoord1 = screenPos;
				
				o.ase_color = v.ase_color;
				o.ase_texcoord = v.ase_texcoord;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = defaultVertexValue;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.positionOS.xyz = vertexValue;
				#else
					v.positionOS.xyz += vertexValue;
				#endif

				VertexPositionInputs vertexInput = GetVertexPositionInputs(v.positionOS.xyz);
				float3 positionWS = TransformObjectToWorld(v.positionOS);
				o.positionCS = TransformWorldToHClip(positionWS);

				return o;
			}

			half4 frag(VertexOutput IN ) : SV_TARGET
			{
				float2 texCoord81 = IN.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float2 panner80 = ( 1.0 * _Time.y * _DetailNoisePanning + texCoord81);
				float4 tex2DNode79 = tex2D( _DetailNoise, panner80 );
				float4 break17_g63 = tex2DNode79;
				float4 appendResult18_g63 = (float4(break17_g63.x , break17_g63.y , break17_g63.z , break17_g63.w));
				float4 clampResult19_g63 = clamp( ( appendResult18_g63 * _DetailDistortionChannel ) , float4( 0,0,0,0 ) , float4( 1,1,1,1 ) );
				float4 break2_g63 = clampResult19_g63;
				float clampResult20_g63 = clamp( ( break2_g63.x + break2_g63.y + break2_g63.z + break2_g63.w ) , 0.0 , 1.0 );
				float DistortionNoise90 = clampResult20_g63;
				float4 screenPos = IN.ase_texcoord1;
				float4 ase_screenPosNorm = screenPos / screenPos.w;
				ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
				float2 panner22 = ( 1.0 * _Time.y * _MainTexturePanning + ( ( DistortionNoise90 * _DistortionStrenght ) + ( (float2( 0,0 ) + ((ase_screenPosNorm).xy - float2( 0,0 )) * (float2( 1,1 ) - float2( 0,0 )) / (float2( 1,1 ) - float2( 0,0 ))) * float2( 4,4 ) ) ));
				float4 tex2DNode150 = tex2D( _MainTex, panner22 );
				float4 break17_g67 = tex2DNode150;
				float4 appendResult18_g67 = (float4(break17_g67.x , break17_g67.y , break17_g67.z , break17_g67.w));
				float4 clampResult19_g67 = clamp( ( appendResult18_g67 * _MainTextureChannel ) , float4( 0,0,0,0 ) , float4( 1,1,1,1 ) );
				float4 break2_g67 = clampResult19_g67;
				float clampResult20_g67 = clamp( ( break2_g67.x + break2_g67.y + break2_g67.z + break2_g67.w ) , 0.0 , 1.0 );
				float3 temp_cast_2 = (clampResult20_g67).xxx;
				float3 desaturateInitialColor158 = temp_cast_2;
				float desaturateDot158 = dot( desaturateInitialColor158, float3( 0.299, 0.587, 0.114 ));
				float3 desaturateVar158 = lerp( desaturateInitialColor158, desaturateDot158.xxx, _Desaturate );
				float4 texCoord162 = IN.ase_texcoord;
				texCoord162.xy = IN.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float4 break17_g65 = tex2DNode79;
				float4 appendResult18_g65 = (float4(break17_g65.x , break17_g65.y , break17_g65.z , break17_g65.w));
				float4 clampResult19_g65 = clamp( ( appendResult18_g65 * _DetailMultiplyChannel ) , float4( 0,0,0,0 ) , float4( 1,1,1,1 ) );
				float4 break2_g65 = clampResult19_g65;
				float clampResult20_g65 = clamp( ( break2_g65.x + break2_g65.y + break2_g65.z + break2_g65.w ) , 0.0 , 1.0 );
				float ifLocalVar106 = 0;
				if( ( _DetailMultiplyChannel.x + _DetailMultiplyChannel.y + _DetailMultiplyChannel.z + _DetailMultiplyChannel.w ) <= 0.0 )
				ifLocalVar106 = 1.0;
				else
				ifLocalVar106 = clampResult20_g65;
				float MultiplyNoise92 = ifLocalVar106;
				float4 break17_g66 = tex2DNode79;
				float4 appendResult18_g66 = (float4(break17_g66.x , break17_g66.y , break17_g66.z , break17_g66.w));
				float4 clampResult19_g66 = clamp( ( appendResult18_g66 * _DetailAdditiveChannel ) , float4( 0,0,0,0 ) , float4( 1,1,1,1 ) );
				float4 break2_g66 = clampResult19_g66;
				float clampResult20_g66 = clamp( ( break2_g66.x + break2_g66.y + break2_g66.z + break2_g66.w ) , 0.0 , 1.0 );
				float AdditiveNoise91 = clampResult20_g66;
				float4 break180 = ( ( IN.ase_color * float4( desaturateVar158 , 0.0 ) * ( texCoord162.z + 1.0 ) * MultiplyNoise92 ) + AdditiveNoise91 );
				float4 texCoord60 = IN.ase_texcoord;
				texCoord60.xy = IN.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float2 texCoord43 = IN.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float2 panner44 = ( 1.0 * _Time.y * _AlphaOverridePanning + texCoord43);
				float4 tex2DNode45 = tex2D( _AlphaOverride, panner44 );
				float4 break2_g64 = ( tex2DNode45 * _AlphaOverrideChannel );
				float AlphaOverride49 = saturate( ( break2_g64.x + break2_g64.y + break2_g64.z + break2_g64.w ) );
				float temp_output_3_0_g68 = ( texCoord60.w - ( 1.0 - AlphaOverride49 ) );
				float4 appendResult181 = (float4(break180.r , break180.g , break180.b , ( IN.ase_color.a * saturate( saturate( ( temp_output_3_0_g68 / fwidth( temp_output_3_0_g68 ) ) ) ) * AlphaOverride49 )));
				
				float4 Color = appendResult181;

				half4 outColor = half4(_ObjectId, _PassValue, 1.0, 1.0);
				return outColor;
			}

            ENDHLSL
        }

		
        Pass
        {
			
            Name "ScenePickingPass"
            Tags { "LightMode"="Picking" }

            Cull Off

            HLSLPROGRAM

			#define ASE_SRP_VERSION 140010


			#pragma vertex vert
			#pragma fragment frag

            #define _SURFACE_TYPE_TRANSPARENT 1
            #define ATTRIBUTES_NEED_NORMAL
            #define ATTRIBUTES_NEED_TANGENT
            #define FEATURES_GRAPH_VERTEX
            #define SHADERPASS SHADERPASS_DEPTHONLY
			#define SCENEPICKINGPASS 1


            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

        	#define ASE_NEEDS_FRAG_COLOR


			sampler2D _MainTex;
			sampler2D _DetailNoise;
			sampler2D _AlphaOverride;
			CBUFFER_START( UnityPerMaterial )
			float4 _DetailDistortionChannel;
			float4 _MainTextureChannel;
			float4 _DetailMultiplyChannel;
			float4 _DetailAdditiveChannel;
			float4 _AlphaOverrideChannel;
			float2 _MainTexturePanning;
			float2 _DetailNoisePanning;
			float2 _AlphaOverridePanning;
			float _DistortionStrenght;
			float _Desaturate;
			CBUFFER_END


            struct VertexInput
			{
				float3 positionOS : POSITION;
				float3 normal : NORMAL;
				float4 tangent : TANGENT;
				float4 ase_color : COLOR;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 positionCS : SV_POSITION;
				float4 ase_color : COLOR;
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_texcoord1 : TEXCOORD1;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

            float4 _SelectionID;

			
			VertexOutput vert(VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );

				float4 ase_clipPos = TransformObjectToHClip((v.positionOS).xyz);
				float4 screenPos = ComputeScreenPos(ase_clipPos);
				o.ase_texcoord1 = screenPos;
				
				o.ase_color = v.ase_color;
				o.ase_texcoord = v.ase_texcoord;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = defaultVertexValue;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.positionOS.xyz = vertexValue;
				#else
					v.positionOS.xyz += vertexValue;
				#endif

				VertexPositionInputs vertexInput = GetVertexPositionInputs(v.positionOS.xyz);
				float3 positionWS = TransformObjectToWorld(v.positionOS);
				o.positionCS = TransformWorldToHClip(positionWS);

				return o;
			}

			half4 frag(VertexOutput IN ) : SV_TARGET
			{
				float2 texCoord81 = IN.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float2 panner80 = ( 1.0 * _Time.y * _DetailNoisePanning + texCoord81);
				float4 tex2DNode79 = tex2D( _DetailNoise, panner80 );
				float4 break17_g63 = tex2DNode79;
				float4 appendResult18_g63 = (float4(break17_g63.x , break17_g63.y , break17_g63.z , break17_g63.w));
				float4 clampResult19_g63 = clamp( ( appendResult18_g63 * _DetailDistortionChannel ) , float4( 0,0,0,0 ) , float4( 1,1,1,1 ) );
				float4 break2_g63 = clampResult19_g63;
				float clampResult20_g63 = clamp( ( break2_g63.x + break2_g63.y + break2_g63.z + break2_g63.w ) , 0.0 , 1.0 );
				float DistortionNoise90 = clampResult20_g63;
				float4 screenPos = IN.ase_texcoord1;
				float4 ase_screenPosNorm = screenPos / screenPos.w;
				ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
				float2 panner22 = ( 1.0 * _Time.y * _MainTexturePanning + ( ( DistortionNoise90 * _DistortionStrenght ) + ( (float2( 0,0 ) + ((ase_screenPosNorm).xy - float2( 0,0 )) * (float2( 1,1 ) - float2( 0,0 )) / (float2( 1,1 ) - float2( 0,0 ))) * float2( 4,4 ) ) ));
				float4 tex2DNode150 = tex2D( _MainTex, panner22 );
				float4 break17_g67 = tex2DNode150;
				float4 appendResult18_g67 = (float4(break17_g67.x , break17_g67.y , break17_g67.z , break17_g67.w));
				float4 clampResult19_g67 = clamp( ( appendResult18_g67 * _MainTextureChannel ) , float4( 0,0,0,0 ) , float4( 1,1,1,1 ) );
				float4 break2_g67 = clampResult19_g67;
				float clampResult20_g67 = clamp( ( break2_g67.x + break2_g67.y + break2_g67.z + break2_g67.w ) , 0.0 , 1.0 );
				float3 temp_cast_2 = (clampResult20_g67).xxx;
				float3 desaturateInitialColor158 = temp_cast_2;
				float desaturateDot158 = dot( desaturateInitialColor158, float3( 0.299, 0.587, 0.114 ));
				float3 desaturateVar158 = lerp( desaturateInitialColor158, desaturateDot158.xxx, _Desaturate );
				float4 texCoord162 = IN.ase_texcoord;
				texCoord162.xy = IN.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float4 break17_g65 = tex2DNode79;
				float4 appendResult18_g65 = (float4(break17_g65.x , break17_g65.y , break17_g65.z , break17_g65.w));
				float4 clampResult19_g65 = clamp( ( appendResult18_g65 * _DetailMultiplyChannel ) , float4( 0,0,0,0 ) , float4( 1,1,1,1 ) );
				float4 break2_g65 = clampResult19_g65;
				float clampResult20_g65 = clamp( ( break2_g65.x + break2_g65.y + break2_g65.z + break2_g65.w ) , 0.0 , 1.0 );
				float ifLocalVar106 = 0;
				if( ( _DetailMultiplyChannel.x + _DetailMultiplyChannel.y + _DetailMultiplyChannel.z + _DetailMultiplyChannel.w ) <= 0.0 )
				ifLocalVar106 = 1.0;
				else
				ifLocalVar106 = clampResult20_g65;
				float MultiplyNoise92 = ifLocalVar106;
				float4 break17_g66 = tex2DNode79;
				float4 appendResult18_g66 = (float4(break17_g66.x , break17_g66.y , break17_g66.z , break17_g66.w));
				float4 clampResult19_g66 = clamp( ( appendResult18_g66 * _DetailAdditiveChannel ) , float4( 0,0,0,0 ) , float4( 1,1,1,1 ) );
				float4 break2_g66 = clampResult19_g66;
				float clampResult20_g66 = clamp( ( break2_g66.x + break2_g66.y + break2_g66.z + break2_g66.w ) , 0.0 , 1.0 );
				float AdditiveNoise91 = clampResult20_g66;
				float4 break180 = ( ( IN.ase_color * float4( desaturateVar158 , 0.0 ) * ( texCoord162.z + 1.0 ) * MultiplyNoise92 ) + AdditiveNoise91 );
				float4 texCoord60 = IN.ase_texcoord;
				texCoord60.xy = IN.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float2 texCoord43 = IN.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float2 panner44 = ( 1.0 * _Time.y * _AlphaOverridePanning + texCoord43);
				float4 tex2DNode45 = tex2D( _AlphaOverride, panner44 );
				float4 break2_g64 = ( tex2DNode45 * _AlphaOverrideChannel );
				float AlphaOverride49 = saturate( ( break2_g64.x + break2_g64.y + break2_g64.z + break2_g64.w ) );
				float temp_output_3_0_g68 = ( texCoord60.w - ( 1.0 - AlphaOverride49 ) );
				float4 appendResult181 = (float4(break180.r , break180.g , break180.b , ( IN.ase_color.a * saturate( saturate( ( temp_output_3_0_g68 / fwidth( temp_output_3_0_g68 ) ) ) ) * AlphaOverride49 )));
				
				float4 Color = appendResult181;
				half4 outColor = _SelectionID;
				return outColor;
			}

            ENDHLSL
        }
		
	}
	CustomEditor "ASEMaterialInspector"
	Fallback "Hidden/InternalErrorShader"
	
}
/*ASEBEGIN
Version=19303
Node;AmplifyShaderEditor.CommentaryNode;103;-784.2378,-2597.328;Inherit;False;1462.886;1030;Extra Noise Setup;14;106;105;86;91;87;92;90;79;85;80;83;81;84;108;;1,1,1,1;0;0
Node;AmplifyShaderEditor.Vector2Node;84;-734.2378,-2214.328;Inherit;False;Property;_DetailNoisePanning;Detail Noise Panning;7;0;Create;True;0;0;0;False;0;False;0,0;0.2,-0.2;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.TextureCoordinatesNode;81;-631.819,-2456.415;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TexturePropertyNode;83;-482.86,-2241.615;Inherit;True;Property;_DetailNoise;Detail Noise;6;0;Create;True;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.PannerNode;80;-391.8206,-2408.415;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;34;-2955.764,-1202.928;Inherit;False;2252.64;1173.84;Main Texture Set Vars;17;149;23;147;27;110;145;146;138;135;22;136;10;111;109;150;151;156;;0,0.5461459,1,1;0;0
Node;AmplifyShaderEditor.Vector4Node;85;-180.2379,-2145.328;Inherit;False;Property;_DetailDistortionChannel;Detail Distortion Channel;8;0;Create;True;0;0;0;False;0;False;1,0,0,0;1,0,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;79;-200.3228,-2353.517;Inherit;True;Property;_TextureSample3;Texture Sample 3;4;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;154;153.6483,-2241.953;Inherit;False;Channel Picker;-1;;63;dc5f4cb24a8bdf448b40a1ec5866280e;0;2;5;FLOAT4;1,0,0,0;False;7;FLOAT4;0,0,0,1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScreenPosInputsNode;145;-2846.101,-404.1232;Float;False;0;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;50;-642.348,-1541.705;Inherit;False;1249.023;565.425;Alpha Override;8;43;44;45;47;48;49;51;157;;0,0.5461459,1,1;0;0
Node;AmplifyShaderEditor.ComponentMaskNode;146;-2604.829,-359.4064;Inherit;False;True;True;False;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;90;416.6483,-2215.953;Inherit;False;DistortionNoise;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;43;-592.348,-1491.705;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector2Node;51;-616.1282,-1132.945;Inherit;False;Property;_AlphaOverridePanning;Alpha Override Panning;4;0;Create;True;0;0;0;False;0;False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.GetLocalVarNode;135;-2339.662,-547.2616;Inherit;False;90;DistortionNoise;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;149;-2394.645,-634.368;Inherit;False;Property;_DistortionStrenght;Distortion Strenght;9;0;Create;True;0;0;0;False;0;False;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;147;-2375.829,-403.4063;Inherit;True;5;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT2;1,1;False;3;FLOAT2;0,0;False;4;FLOAT2;1,1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PannerNode;44;-352.3488,-1443.705;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TexturePropertyNode;47;-399.7247,-1273.259;Inherit;True;Property;_AlphaOverride;Alpha Override;3;0;Create;True;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;136;-2124.063,-659.4615;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;151;-2072.593,-317.8097;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;4,4;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;45;-160.851,-1388.807;Inherit;True;Property;_TextureSample2;Texture Sample 2;4;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector4Node;48;-97.32484,-1188.281;Inherit;False;Property;_AlphaOverrideChannel;Alpha Override Channel;5;0;Create;True;0;0;0;False;0;False;1,0,0,0;1,0,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;138;-1995.662,-662.2618;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector2Node;23;-2017.901,-501.3873;Inherit;False;Property;_MainTexturePanning;Main Texture Panning;2;0;Create;True;0;0;0;False;0;False;0,0;0.2,0.2;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.Vector4Node;86;-593.2379,-1899.328;Inherit;False;Property;_DetailMultiplyChannel;Detail Multiply Channel;10;0;Create;True;0;0;0;False;0;False;0,0,0,0;0,0,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;157;239.2312,-1200.401;Inherit;False;Channel Picker Alpha;-1;;64;e49841402b321534583d1dc019041b68;0;2;5;FLOAT4;1,0,0,0;False;7;FLOAT4;0,0,0,1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;105;-231.1713,-1862.338;Inherit;False;4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;153;-13.35168,-1929.953;Inherit;False;Channel Picker;-1;;65;dc5f4cb24a8bdf448b40a1ec5866280e;0;2;5;FLOAT4;1,0,0,0;False;7;FLOAT4;0,0,0,1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;108;-228.1713,-1713.338;Inherit;False;Constant;_Float0;Float 0;15;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;22;-1683.138,-518.7701;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TexturePropertyNode;27;-1773.575,-855.0742;Inherit;True;Property;_MainTex;Main Texture;0;0;Create;False;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.RegisterLocalVarNode;49;382.6752,-1380.28;Inherit;False;AlphaOverride;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector4Node;10;-1419.139,-362.7703;Inherit;False;Property;_MainTextureChannel;Main Texture Channel;1;0;Create;True;0;0;0;False;0;False;1,1,1,0;1,1,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;150;-1508.96,-631.8519;Inherit;True;Property;_TextureSample0;Texture Sample 0;12;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector4Node;87;-153.2379,-2547.328;Inherit;False;Property;_DetailAdditiveChannel;Detail Additive Channel;11;0;Create;True;0;0;0;False;0;False;0,0,0,0;0,0,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ConditionalIfNode;106;259.8287,-1914.338;Inherit;False;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;52;-304.4512,205.567;Inherit;False;49;AlphaOverride;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;159;-462.6389,-257.7749;Inherit;False;Property;_Desaturate;Desaturate?;12;0;Create;True;0;0;0;False;0;False;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;162;-382.7326,-134.1047;Inherit;False;0;-1;4;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;155;132.2033,-2464.99;Inherit;False;Channel Picker;-1;;66;dc5f4cb24a8bdf448b40a1ec5866280e;0;2;5;FLOAT4;1,0,0,0;False;7;FLOAT4;0,0,0,1;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;156;-1069.835,-363.9704;Inherit;False;Channel Picker;-1;;67;dc5f4cb24a8bdf448b40a1ec5866280e;0;2;5;FLOAT4;1,0,0,0;False;7;FLOAT4;0,0,0,1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;92;431.6483,-1884.953;Inherit;False;MultiplyNoise;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;64;31.54882,205.567;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;60;15.54882,285.5669;Inherit;False;0;-1;4;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.VertexColorNode;37;-168.5735,-497.7309;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DesaturateOpNode;158;-165.6389,-328.7749;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;163;-149.0954,-220.5375;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;98;-292.8669,32.71658;Inherit;False;92;MultiplyNoise;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;91;400.6483,-2501.953;Inherit;False;AdditiveNoise;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;70;287.5488,253.5669;Inherit;False;Step Antialiasing;-1;;68;2a825e80dfb3290468194f83380797bd;0;2;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;164;170.5334,-373.1786;Inherit;False;4;4;0;COLOR;0,0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;101;136.5273,-200.7529;Inherit;False;91;AdditiveNoise;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;68;495.5488,285.5669;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;61;270.9582,23.08784;Inherit;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;102;408.6009,-362.8981;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;40;627.2988,159.4486;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.BreakToComponentsNode;180;536.9998,-267.677;Inherit;False;COLOR;1;0;COLOR;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.BreakToComponentsNode;110;-1053,-596.252;Inherit;False;COLOR;1;0;COLOR;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;109;-1181,-596.252;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.DynamicAppendNode;111;-864.0005,-549.252;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.FunctionNode;152;158.6752,-1380.28;Inherit;False;Channel Picker;-1;;69;dc5f4cb24a8bdf448b40a1ec5866280e;0;2;5;FLOAT4;1,0,0,0;False;7;FLOAT4;0,0,0,1;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;181;802.3332,-174.3436;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;177;849.0098,-373.7703;Float;False;False;-1;2;ASEMaterialInspector;0;1;New Amplify Shader;cf964e524c8e69742b1d21fbe2ebcc4a;True;Sprite Unlit Forward;0;1;Sprite Unlit Forward;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Transparent=RenderType;Queue=Transparent=Queue=0;True;0;True;12;all;0;False;True;2;5;False;;10;False;;3;1;False;;10;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;2;False;;True;3;False;;True;True;0;False;;0;False;;True;1;LightMode=UniversalForward;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;178;849.0098,-373.7703;Float;False;False;-1;2;ASEMaterialInspector;0;1;New Amplify Shader;cf964e524c8e69742b1d21fbe2ebcc4a;True;SceneSelectionPass;0;2;SceneSelectionPass;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Transparent=RenderType;Queue=Transparent=Queue=0;True;0;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=SceneSelectionPass;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;179;849.0098,-373.7703;Float;False;False;-1;2;ASEMaterialInspector;0;1;New Amplify Shader;cf964e524c8e69742b1d21fbe2ebcc4a;True;ScenePickingPass;0;3;ScenePickingPass;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Transparent=RenderType;Queue=Transparent=Queue=0;True;0;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=Picking;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;176;1045.01,-280.437;Float;False;True;-1;2;ASEMaterialInspector;0;15;Piloto Studio/Screenspace Alpha;cf964e524c8e69742b1d21fbe2ebcc4a;True;Sprite Unlit;0;0;Sprite Unlit;4;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Transparent=RenderType;Queue=Transparent=Queue=0;True;0;True;12;all;0;False;True;2;5;False;;10;False;;3;1;False;;10;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;2;False;;True;3;False;;True;True;0;False;;0;False;;True;1;LightMode=Universal2D;False;False;0;Hidden/InternalErrorShader;0;0;Standard;3;Vertex Position;1;0;Debug Display;0;0;External Alpha;0;0;0;4;True;True;True;True;False;;False;0
WireConnection;80;0;81;0
WireConnection;80;2;84;0
WireConnection;79;0;83;0
WireConnection;79;1;80;0
WireConnection;154;5;79;0
WireConnection;154;7;85;0
WireConnection;146;0;145;0
WireConnection;90;0;154;0
WireConnection;147;0;146;0
WireConnection;44;0;43;0
WireConnection;44;2;51;0
WireConnection;136;0;135;0
WireConnection;136;1;149;0
WireConnection;151;0;147;0
WireConnection;45;0;47;0
WireConnection;45;1;44;0
WireConnection;138;0;136;0
WireConnection;138;1;151;0
WireConnection;157;5;45;0
WireConnection;157;7;48;0
WireConnection;105;0;86;1
WireConnection;105;1;86;2
WireConnection;105;2;86;3
WireConnection;105;3;86;4
WireConnection;153;5;79;0
WireConnection;153;7;86;0
WireConnection;22;0;138;0
WireConnection;22;2;23;0
WireConnection;49;0;157;0
WireConnection;150;0;27;0
WireConnection;150;1;22;0
WireConnection;106;0;105;0
WireConnection;106;2;153;0
WireConnection;106;3;108;0
WireConnection;106;4;108;0
WireConnection;155;5;79;0
WireConnection;155;7;87;0
WireConnection;156;5;150;0
WireConnection;156;7;10;0
WireConnection;92;0;106;0
WireConnection;64;0;52;0
WireConnection;158;0;156;0
WireConnection;158;1;159;0
WireConnection;163;0;162;3
WireConnection;91;0;155;0
WireConnection;70;1;64;0
WireConnection;70;2;60;4
WireConnection;164;0;37;0
WireConnection;164;1;158;0
WireConnection;164;2;163;0
WireConnection;164;3;98;0
WireConnection;68;0;70;0
WireConnection;102;0;164;0
WireConnection;102;1;101;0
WireConnection;40;0;61;4
WireConnection;40;1;68;0
WireConnection;40;2;52;0
WireConnection;180;0;102;0
WireConnection;110;0;109;0
WireConnection;109;0;150;0
WireConnection;109;1;10;0
WireConnection;111;0;110;0
WireConnection;111;1;110;1
WireConnection;111;2;110;2
WireConnection;111;3;110;3
WireConnection;152;5;45;0
WireConnection;152;7;48;0
WireConnection;181;0;180;0
WireConnection;181;1;180;1
WireConnection;181;2;180;2
WireConnection;181;3;40;0
WireConnection;176;1;181;0
ASEEND*/
//CHKSM=46B2A42991892843BCC1798F83B33408926D5B9E