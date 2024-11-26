// Made with Amplify Shader Editor v1.9.3.3
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Piloto Studio/Additive Uber Shader"
{
	Properties
	{
		[HideInInspector] _EmissionColor("Emission Color", Color) = (1,1,1,1)
		[HideInInspector] _AlphaCutoff("Alpha Cutoff ", Range(0, 1)) = 0.5
		_MainTex("Main Texture", 2D) = "white" {}
		_Desaturate("Desaturate?", Range( 0 , 1)) = 0
		_MainTextureChannel("Main Texture Channel", Vector) = (1,1,1,0)
		_MainAlphaChannel("Main Alpha Channel", Vector) = (0,0,0,1)
		_MainTexturePanning("Main Texture Panning", Vector) = (0,0,0,0)
		_MainAlphaPanning("Main Alpha Panning", Vector) = (0,0,0,0)
		_AlphaOverride("Alpha Override", 2D) = "white" {}
		_AlphaOverridePanning("Alpha Override Panning", Vector) = (0,0,0,0)
		_AlphaOverrideChannel("Alpha Override Channel", Vector) = (1,0,0,0)
		_FlipbooksColumsRows("Flipbooks Colums & Rows", Vector) = (1,1,0,0)
		_DetailNoise("Detail Noise", 2D) = "white" {}
		_DetailNoisePanning("Detail Noise Panning", Vector) = (0,0,0,0)
		_DetailDistortionChannel("Detail Distortion Channel", Vector) = (1,0,0,0)
		_DistortionIntensity("Distortion Intensity", Float) = 0
		_DetailMultiplyChannel("Detail Multiply Channel", Vector) = (0,0,0,0)
		_DetailAdditiveChannel("Detail Additive Channel", Vector) = (0,0,0,0)
		[Toggle(_USESOFTALPHA_ON)] _UseSoftAlpha("UseSoftAlpha", Float) = 0
		_SoftFadeFactor("SoftFadeFactor", Range( 0.1 , 1)) = 0.1

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
			#define REQUIRE_DEPTH_TEXTURE 1


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
			#pragma shader_feature_local _USESOFTALPHA_ON


			sampler2D _MainTex;
			sampler2D _DetailNoise;
			sampler2D _AlphaOverride;
			CBUFFER_START( UnityPerMaterial )
			float4 _DetailNoise_ST;
			float4 _DetailDistortionChannel;
			float4 _MainTextureChannel;
			float4 _DetailAdditiveChannel;
			float4 _DetailMultiplyChannel;
			float4 _AlphaOverride_ST;
			float4 _AlphaOverrideChannel;
			float4 _MainAlphaChannel;
			float2 _MainTexturePanning;
			float2 _DetailNoisePanning;
			float2 _FlipbooksColumsRows;
			float2 _AlphaOverridePanning;
			float2 _MainAlphaPanning;
			float _DistortionIntensity;
			float _Desaturate;
			float _SoftFadeFactor;
			CBUFFER_END


			struct VertexInput
			{
				float4 positionOS : POSITION;
				float3 normal : NORMAL;
				float4 tangent : TANGENT;
				float4 uv0 : TEXCOORD0;
				float4 color : COLOR;
				float4 ase_texcoord1 : TEXCOORD1;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 positionCS : SV_POSITION;
				float4 texCoord0 : TEXCOORD0;
				float4 color : TEXCOORD1;
				float3 positionWS : TEXCOORD2;
				float4 ase_texcoord3 : TEXCOORD3;
				float4 ase_texcoord4 : TEXCOORD4;
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
				o.ase_texcoord4 = screenPos;
				
				o.ase_texcoord3 = v.ase_texcoord1;
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

				float2 uv_DetailNoise = IN.texCoord0.xy * _DetailNoise_ST.xy + _DetailNoise_ST.zw;
				float2 panner209 = ( 1.0 * _Time.y * _DetailNoisePanning + uv_DetailNoise);
				float4 tex2DNode211 = tex2D( _DetailNoise, panner209 );
				float4 break17_g136 = tex2DNode211;
				float4 appendResult18_g136 = (float4(break17_g136.x , break17_g136.y , break17_g136.z , break17_g136.w));
				float4 clampResult19_g136 = clamp( ( appendResult18_g136 * _DetailDistortionChannel ) , float4( 0,0,0,0 ) , float4( 1,1,1,1 ) );
				float4 break2_g136 = clampResult19_g136;
				float clampResult20_g136 = clamp( ( break2_g136.x + break2_g136.y + break2_g136.z + break2_g136.w ) , 0.0 , 1.0 );
				float3 temp_cast_1 = (clampResult20_g136).xxx;
				float3 desaturateInitialColor213 = temp_cast_1;
				float desaturateDot213 = dot( desaturateInitialColor213, float3( 0.299, 0.587, 0.114 ));
				float3 desaturateVar213 = lerp( desaturateInitialColor213, desaturateDot213.xxx, 1.0 );
				float3 DistortionNoise214 = desaturateVar213;
				float4 texCoord192 = IN.ase_texcoord3;
				texCoord192.xy = IN.ase_texcoord3.xy * float2( 1,1 ) + float2( 0,0 );
				float2 appendResult194 = (float2(texCoord192.z , 0.0));
				float2 appendResult195 = (float2(0.0 , 0.0));
				float2 LocalUVOffset197 = ( appendResult194 + appendResult195 );
				float2 texCoord200 = IN.texCoord0.xy * float2( 1,1 ) + float2( 0,0 );
				float3 UVFlipbookInput205 = ( ( DistortionNoise214 * _DistortionIntensity ) + float3( ( LocalUVOffset197 + texCoord200 ) ,  0.0 ) );
				float2 break135_g140 = UVFlipbookInput205.xy;
				float2 appendResult206_g140 = (float2(frac( break135_g140.x ) , frac( break135_g140.y )));
				float temp_output_4_0_g140 = _FlipbooksColumsRows.x;
				float temp_output_5_0_g140 = _FlipbooksColumsRows.y;
				float2 appendResult116_g140 = (float2(temp_output_4_0_g140 , temp_output_5_0_g140));
				float temp_output_122_0_g140 = ( temp_output_4_0_g140 * temp_output_5_0_g140 );
				float2 appendResult175_g140 = (float2(temp_output_122_0_g140 , temp_output_5_0_g140));
				float Columns213_g140 = temp_output_4_0_g140;
				float Rows212_g140 = temp_output_5_0_g140;
				float temp_output_133_0_g140 = ( fmod( _TimeParameters.x , ( Columns213_g140 * Rows212_g140 ) ) * 0.0 );
				float4 texCoord218 = IN.ase_texcoord3;
				texCoord218.xy = IN.ase_texcoord3.xy * float2( 1,1 ) + float2( 0,0 );
				float clampResult129_g140 = clamp( texCoord218.x , 1E-05 , ( temp_output_122_0_g140 - 1.0 ) );
				float temp_output_185_0_g140 = frac( ( ( temp_output_133_0_g140 + ( clampResult129_g140 + 1E-05 ) ) / temp_output_122_0_g140 ) );
				float2 appendResult186_g140 = (float2(temp_output_185_0_g140 , ( 1.0 - temp_output_185_0_g140 )));
				float2 temp_output_203_0_g140 = ( ( appendResult206_g140 / appendResult116_g140 ) + ( floor( ( appendResult175_g140 * appendResult186_g140 ) ) / appendResult116_g140 ) );
				float2 temp_output_225_0 = temp_output_203_0_g140;
				float2 panner177 = ( 1.0 * _Time.y * _MainTexturePanning + temp_output_225_0);
				float4 break17_g144 = tex2D( _MainTex, panner177 );
				float4 appendResult18_g144 = (float4(break17_g144.x , break17_g144.y , break17_g144.z , break17_g144.w));
				float4 clampResult19_g144 = clamp( ( appendResult18_g144 * _MainTextureChannel ) , float4( 0,0,0,0 ) , float4( 1,1,1,1 ) );
				float4 break2_g144 = clampResult19_g144;
				float clampResult20_g144 = clamp( ( break2_g144.x + break2_g144.y + break2_g144.z + break2_g144.w ) , 0.0 , 1.0 );
				float MainTexInfo180 = clampResult20_g144;
				float3 temp_cast_5 = (MainTexInfo180).xxx;
				float3 desaturateInitialColor175 = temp_cast_5;
				float desaturateDot175 = dot( desaturateInitialColor175, float3( 0.299, 0.587, 0.114 ));
				float3 desaturateVar175 = lerp( desaturateInitialColor175, desaturateDot175.xxx, _Desaturate );
				float4 break243 = ( _DetailAdditiveChannel * tex2DNode211 );
				float4 appendResult237 = (float4(break243.x , break243.y , break243.z , break243.w));
				float3 desaturateInitialColor241 = appendResult237.xyz;
				float desaturateDot241 = dot( desaturateInitialColor241, float3( 0.299, 0.587, 0.114 ));
				float3 desaturateVar241 = lerp( desaturateInitialColor241, desaturateDot241.xxx, 1.0 );
				float3 AdditiveNoise245 = desaturateVar241;
				float4 texCoord184 = IN.texCoord0;
				texCoord184.xy = IN.texCoord0.xy * float2( 1,1 ) + float2( 0,0 );
				float4 break17_g145 = tex2DNode211;
				float4 appendResult18_g145 = (float4(break17_g145.x , break17_g145.y , break17_g145.z , break17_g145.w));
				float4 clampResult19_g145 = clamp( ( appendResult18_g145 * _DetailMultiplyChannel ) , float4( 0,0,0,0 ) , float4( 1,1,1,1 ) );
				float4 break2_g145 = clampResult19_g145;
				float clampResult20_g145 = clamp( ( break2_g145.x + break2_g145.y + break2_g145.z + break2_g145.w ) , 0.0 , 1.0 );
				float ifLocalVar238 = 0;
				if( ( _DetailMultiplyChannel.x + _DetailMultiplyChannel.y + _DetailMultiplyChannel.z + _DetailMultiplyChannel.w ) <= 0.0 )
				ifLocalVar238 = 1.0;
				else
				ifLocalVar238 = clampResult20_g145;
				float3 temp_cast_10 = (ifLocalVar238).xxx;
				float3 desaturateInitialColor236 = temp_cast_10;
				float desaturateDot236 = dot( desaturateInitialColor236, float3( 0.299, 0.587, 0.114 ));
				float3 desaturateVar236 = lerp( desaturateInitialColor236, desaturateDot236.xxx, 1.0 );
				float3 MultiplyNoise246 = desaturateVar236;
				float2 uv_AlphaOverride = IN.texCoord0.xy * _AlphaOverride_ST.xy + _AlphaOverride_ST.zw;
				float2 break135_g139 = ( LocalUVOffset197 + uv_AlphaOverride );
				float2 appendResult206_g139 = (float2(frac( break135_g139.x ) , frac( break135_g139.y )));
				float temp_output_4_0_g139 = _FlipbooksColumsRows.x;
				float temp_output_5_0_g139 = _FlipbooksColumsRows.y;
				float2 appendResult116_g139 = (float2(temp_output_4_0_g139 , temp_output_5_0_g139));
				float temp_output_122_0_g139 = ( temp_output_4_0_g139 * temp_output_5_0_g139 );
				float2 appendResult175_g139 = (float2(temp_output_122_0_g139 , temp_output_5_0_g139));
				float Columns213_g139 = temp_output_4_0_g139;
				float Rows212_g139 = temp_output_5_0_g139;
				float temp_output_133_0_g139 = ( fmod( _TimeParameters.x , ( Columns213_g139 * Rows212_g139 ) ) * 0.0 );
				float clampResult129_g139 = clamp( texCoord218.x , 1E-05 , ( temp_output_122_0_g139 - 1.0 ) );
				float temp_output_185_0_g139 = frac( ( ( temp_output_133_0_g139 + ( clampResult129_g139 + 1E-05 ) ) / temp_output_122_0_g139 ) );
				float2 appendResult186_g139 = (float2(temp_output_185_0_g139 , ( 1.0 - temp_output_185_0_g139 )));
				float2 temp_output_203_0_g139 = ( ( appendResult206_g139 / appendResult116_g139 ) + ( floor( ( appendResult175_g139 * appendResult186_g139 ) ) / appendResult116_g139 ) );
				float2 panner227 = ( 1.0 * _Time.y * _AlphaOverridePanning + temp_output_203_0_g139);
				float4 break2_g141 = ( tex2D( _AlphaOverride, panner227 ) * _AlphaOverrideChannel );
				float AlphaOverride234 = saturate( ( break2_g141.x + break2_g141.y + break2_g141.z + break2_g141.w ) );
				float2 panner226 = ( 1.0 * _Time.y * _MainAlphaPanning + temp_output_225_0);
				float4 break2_g142 = ( tex2D( _MainTex, panner226 ) * _MainAlphaChannel );
				float MainAlpha233 = saturate( ( break2_g142.x + break2_g142.y + break2_g142.z + break2_g142.w ) );
				float temp_output_169_0 = ( AlphaOverride234 * MainAlpha233 );
				float4 texCoord171 = IN.texCoord0;
				texCoord171.xy = IN.texCoord0.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g146 = ( texCoord171.w - ( 1.0 - temp_output_169_0 ) );
				float temp_output_188_0 = ( IN.color.a * temp_output_169_0 * saturate( saturate( ( temp_output_3_0_g146 / fwidth( temp_output_3_0_g146 ) ) ) ) );
				float4 screenPos = IN.ase_texcoord4;
				float4 ase_screenPosNorm = screenPos / screenPos.w;
				ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
				float screenDepth251 = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH( ase_screenPosNorm.xy ),_ZBufferParams);
				float distanceDepth251 = abs( ( screenDepth251 - LinearEyeDepth( ase_screenPosNorm.z,_ZBufferParams ) ) / ( _SoftFadeFactor ) );
				#ifdef _USESOFTALPHA_ON
				float staticSwitch249 = ( temp_output_188_0 * saturate( distanceDepth251 ) );
				#else
				float staticSwitch249 = temp_output_188_0;
				#endif
				float4 break265 = ( ( IN.color * float4( ( desaturateVar175 + AdditiveNoise245 ) , 0.0 ) * ( texCoord184.z + 1.0 ) * float4( MultiplyNoise246 , 0.0 ) ) * staticSwitch249 );
				float4 appendResult267 = (float4(break265.r , break265.g , break265.b , staticSwitch249));
				
				float4 Color = appendResult267;

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
			#define REQUIRE_DEPTH_TEXTURE 1


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
			#pragma shader_feature_local _USESOFTALPHA_ON


			sampler2D _MainTex;
			sampler2D _DetailNoise;
			sampler2D _AlphaOverride;
			CBUFFER_START( UnityPerMaterial )
			float4 _DetailNoise_ST;
			float4 _DetailDistortionChannel;
			float4 _MainTextureChannel;
			float4 _DetailAdditiveChannel;
			float4 _DetailMultiplyChannel;
			float4 _AlphaOverride_ST;
			float4 _AlphaOverrideChannel;
			float4 _MainAlphaChannel;
			float2 _MainTexturePanning;
			float2 _DetailNoisePanning;
			float2 _FlipbooksColumsRows;
			float2 _AlphaOverridePanning;
			float2 _MainAlphaPanning;
			float _DistortionIntensity;
			float _Desaturate;
			float _SoftFadeFactor;
			CBUFFER_END


			struct VertexInput
			{
				float4 positionOS : POSITION;
				float3 normal : NORMAL;
				float4 tangent : TANGENT;
				float4 uv0 : TEXCOORD0;
				float4 color : COLOR;
				float4 ase_texcoord1 : TEXCOORD1;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 positionCS : SV_POSITION;
				float4 texCoord0 : TEXCOORD0;
				float4 color : TEXCOORD1;
				float3 positionWS : TEXCOORD2;
				float4 ase_texcoord3 : TEXCOORD3;
				float4 ase_texcoord4 : TEXCOORD4;
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
				o.ase_texcoord4 = screenPos;
				
				o.ase_texcoord3 = v.ase_texcoord1;
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

				float2 uv_DetailNoise = IN.texCoord0.xy * _DetailNoise_ST.xy + _DetailNoise_ST.zw;
				float2 panner209 = ( 1.0 * _Time.y * _DetailNoisePanning + uv_DetailNoise);
				float4 tex2DNode211 = tex2D( _DetailNoise, panner209 );
				float4 break17_g136 = tex2DNode211;
				float4 appendResult18_g136 = (float4(break17_g136.x , break17_g136.y , break17_g136.z , break17_g136.w));
				float4 clampResult19_g136 = clamp( ( appendResult18_g136 * _DetailDistortionChannel ) , float4( 0,0,0,0 ) , float4( 1,1,1,1 ) );
				float4 break2_g136 = clampResult19_g136;
				float clampResult20_g136 = clamp( ( break2_g136.x + break2_g136.y + break2_g136.z + break2_g136.w ) , 0.0 , 1.0 );
				float3 temp_cast_1 = (clampResult20_g136).xxx;
				float3 desaturateInitialColor213 = temp_cast_1;
				float desaturateDot213 = dot( desaturateInitialColor213, float3( 0.299, 0.587, 0.114 ));
				float3 desaturateVar213 = lerp( desaturateInitialColor213, desaturateDot213.xxx, 1.0 );
				float3 DistortionNoise214 = desaturateVar213;
				float4 texCoord192 = IN.ase_texcoord3;
				texCoord192.xy = IN.ase_texcoord3.xy * float2( 1,1 ) + float2( 0,0 );
				float2 appendResult194 = (float2(texCoord192.z , 0.0));
				float2 appendResult195 = (float2(0.0 , 0.0));
				float2 LocalUVOffset197 = ( appendResult194 + appendResult195 );
				float2 texCoord200 = IN.texCoord0.xy * float2( 1,1 ) + float2( 0,0 );
				float3 UVFlipbookInput205 = ( ( DistortionNoise214 * _DistortionIntensity ) + float3( ( LocalUVOffset197 + texCoord200 ) ,  0.0 ) );
				float2 break135_g140 = UVFlipbookInput205.xy;
				float2 appendResult206_g140 = (float2(frac( break135_g140.x ) , frac( break135_g140.y )));
				float temp_output_4_0_g140 = _FlipbooksColumsRows.x;
				float temp_output_5_0_g140 = _FlipbooksColumsRows.y;
				float2 appendResult116_g140 = (float2(temp_output_4_0_g140 , temp_output_5_0_g140));
				float temp_output_122_0_g140 = ( temp_output_4_0_g140 * temp_output_5_0_g140 );
				float2 appendResult175_g140 = (float2(temp_output_122_0_g140 , temp_output_5_0_g140));
				float Columns213_g140 = temp_output_4_0_g140;
				float Rows212_g140 = temp_output_5_0_g140;
				float temp_output_133_0_g140 = ( fmod( _TimeParameters.x , ( Columns213_g140 * Rows212_g140 ) ) * 0.0 );
				float4 texCoord218 = IN.ase_texcoord3;
				texCoord218.xy = IN.ase_texcoord3.xy * float2( 1,1 ) + float2( 0,0 );
				float clampResult129_g140 = clamp( texCoord218.x , 1E-05 , ( temp_output_122_0_g140 - 1.0 ) );
				float temp_output_185_0_g140 = frac( ( ( temp_output_133_0_g140 + ( clampResult129_g140 + 1E-05 ) ) / temp_output_122_0_g140 ) );
				float2 appendResult186_g140 = (float2(temp_output_185_0_g140 , ( 1.0 - temp_output_185_0_g140 )));
				float2 temp_output_203_0_g140 = ( ( appendResult206_g140 / appendResult116_g140 ) + ( floor( ( appendResult175_g140 * appendResult186_g140 ) ) / appendResult116_g140 ) );
				float2 temp_output_225_0 = temp_output_203_0_g140;
				float2 panner177 = ( 1.0 * _Time.y * _MainTexturePanning + temp_output_225_0);
				float4 break17_g144 = tex2D( _MainTex, panner177 );
				float4 appendResult18_g144 = (float4(break17_g144.x , break17_g144.y , break17_g144.z , break17_g144.w));
				float4 clampResult19_g144 = clamp( ( appendResult18_g144 * _MainTextureChannel ) , float4( 0,0,0,0 ) , float4( 1,1,1,1 ) );
				float4 break2_g144 = clampResult19_g144;
				float clampResult20_g144 = clamp( ( break2_g144.x + break2_g144.y + break2_g144.z + break2_g144.w ) , 0.0 , 1.0 );
				float MainTexInfo180 = clampResult20_g144;
				float3 temp_cast_5 = (MainTexInfo180).xxx;
				float3 desaturateInitialColor175 = temp_cast_5;
				float desaturateDot175 = dot( desaturateInitialColor175, float3( 0.299, 0.587, 0.114 ));
				float3 desaturateVar175 = lerp( desaturateInitialColor175, desaturateDot175.xxx, _Desaturate );
				float4 break243 = ( _DetailAdditiveChannel * tex2DNode211 );
				float4 appendResult237 = (float4(break243.x , break243.y , break243.z , break243.w));
				float3 desaturateInitialColor241 = appendResult237.xyz;
				float desaturateDot241 = dot( desaturateInitialColor241, float3( 0.299, 0.587, 0.114 ));
				float3 desaturateVar241 = lerp( desaturateInitialColor241, desaturateDot241.xxx, 1.0 );
				float3 AdditiveNoise245 = desaturateVar241;
				float4 texCoord184 = IN.texCoord0;
				texCoord184.xy = IN.texCoord0.xy * float2( 1,1 ) + float2( 0,0 );
				float4 break17_g145 = tex2DNode211;
				float4 appendResult18_g145 = (float4(break17_g145.x , break17_g145.y , break17_g145.z , break17_g145.w));
				float4 clampResult19_g145 = clamp( ( appendResult18_g145 * _DetailMultiplyChannel ) , float4( 0,0,0,0 ) , float4( 1,1,1,1 ) );
				float4 break2_g145 = clampResult19_g145;
				float clampResult20_g145 = clamp( ( break2_g145.x + break2_g145.y + break2_g145.z + break2_g145.w ) , 0.0 , 1.0 );
				float ifLocalVar238 = 0;
				if( ( _DetailMultiplyChannel.x + _DetailMultiplyChannel.y + _DetailMultiplyChannel.z + _DetailMultiplyChannel.w ) <= 0.0 )
				ifLocalVar238 = 1.0;
				else
				ifLocalVar238 = clampResult20_g145;
				float3 temp_cast_10 = (ifLocalVar238).xxx;
				float3 desaturateInitialColor236 = temp_cast_10;
				float desaturateDot236 = dot( desaturateInitialColor236, float3( 0.299, 0.587, 0.114 ));
				float3 desaturateVar236 = lerp( desaturateInitialColor236, desaturateDot236.xxx, 1.0 );
				float3 MultiplyNoise246 = desaturateVar236;
				float2 uv_AlphaOverride = IN.texCoord0.xy * _AlphaOverride_ST.xy + _AlphaOverride_ST.zw;
				float2 break135_g139 = ( LocalUVOffset197 + uv_AlphaOverride );
				float2 appendResult206_g139 = (float2(frac( break135_g139.x ) , frac( break135_g139.y )));
				float temp_output_4_0_g139 = _FlipbooksColumsRows.x;
				float temp_output_5_0_g139 = _FlipbooksColumsRows.y;
				float2 appendResult116_g139 = (float2(temp_output_4_0_g139 , temp_output_5_0_g139));
				float temp_output_122_0_g139 = ( temp_output_4_0_g139 * temp_output_5_0_g139 );
				float2 appendResult175_g139 = (float2(temp_output_122_0_g139 , temp_output_5_0_g139));
				float Columns213_g139 = temp_output_4_0_g139;
				float Rows212_g139 = temp_output_5_0_g139;
				float temp_output_133_0_g139 = ( fmod( _TimeParameters.x , ( Columns213_g139 * Rows212_g139 ) ) * 0.0 );
				float clampResult129_g139 = clamp( texCoord218.x , 1E-05 , ( temp_output_122_0_g139 - 1.0 ) );
				float temp_output_185_0_g139 = frac( ( ( temp_output_133_0_g139 + ( clampResult129_g139 + 1E-05 ) ) / temp_output_122_0_g139 ) );
				float2 appendResult186_g139 = (float2(temp_output_185_0_g139 , ( 1.0 - temp_output_185_0_g139 )));
				float2 temp_output_203_0_g139 = ( ( appendResult206_g139 / appendResult116_g139 ) + ( floor( ( appendResult175_g139 * appendResult186_g139 ) ) / appendResult116_g139 ) );
				float2 panner227 = ( 1.0 * _Time.y * _AlphaOverridePanning + temp_output_203_0_g139);
				float4 break2_g141 = ( tex2D( _AlphaOverride, panner227 ) * _AlphaOverrideChannel );
				float AlphaOverride234 = saturate( ( break2_g141.x + break2_g141.y + break2_g141.z + break2_g141.w ) );
				float2 panner226 = ( 1.0 * _Time.y * _MainAlphaPanning + temp_output_225_0);
				float4 break2_g142 = ( tex2D( _MainTex, panner226 ) * _MainAlphaChannel );
				float MainAlpha233 = saturate( ( break2_g142.x + break2_g142.y + break2_g142.z + break2_g142.w ) );
				float temp_output_169_0 = ( AlphaOverride234 * MainAlpha233 );
				float4 texCoord171 = IN.texCoord0;
				texCoord171.xy = IN.texCoord0.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g146 = ( texCoord171.w - ( 1.0 - temp_output_169_0 ) );
				float temp_output_188_0 = ( IN.color.a * temp_output_169_0 * saturate( saturate( ( temp_output_3_0_g146 / fwidth( temp_output_3_0_g146 ) ) ) ) );
				float4 screenPos = IN.ase_texcoord4;
				float4 ase_screenPosNorm = screenPos / screenPos.w;
				ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
				float screenDepth251 = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH( ase_screenPosNorm.xy ),_ZBufferParams);
				float distanceDepth251 = abs( ( screenDepth251 - LinearEyeDepth( ase_screenPosNorm.z,_ZBufferParams ) ) / ( _SoftFadeFactor ) );
				#ifdef _USESOFTALPHA_ON
				float staticSwitch249 = ( temp_output_188_0 * saturate( distanceDepth251 ) );
				#else
				float staticSwitch249 = temp_output_188_0;
				#endif
				float4 break265 = ( ( IN.color * float4( ( desaturateVar175 + AdditiveNoise245 ) , 0.0 ) * ( texCoord184.z + 1.0 ) * float4( MultiplyNoise246 , 0.0 ) ) * staticSwitch249 );
				float4 appendResult267 = (float4(break265.r , break265.g , break265.b , staticSwitch249));
				
				float4 Color = appendResult267;

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
			#define REQUIRE_DEPTH_TEXTURE 1


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
			#pragma shader_feature_local _USESOFTALPHA_ON


			sampler2D _MainTex;
			sampler2D _DetailNoise;
			sampler2D _AlphaOverride;
			CBUFFER_START( UnityPerMaterial )
			float4 _DetailNoise_ST;
			float4 _DetailDistortionChannel;
			float4 _MainTextureChannel;
			float4 _DetailAdditiveChannel;
			float4 _DetailMultiplyChannel;
			float4 _AlphaOverride_ST;
			float4 _AlphaOverrideChannel;
			float4 _MainAlphaChannel;
			float2 _MainTexturePanning;
			float2 _DetailNoisePanning;
			float2 _FlipbooksColumsRows;
			float2 _AlphaOverridePanning;
			float2 _MainAlphaPanning;
			float _DistortionIntensity;
			float _Desaturate;
			float _SoftFadeFactor;
			CBUFFER_END


            struct VertexInput
			{
				float3 positionOS : POSITION;
				float3 normal : NORMAL;
				float4 tangent : TANGENT;
				float4 ase_color : COLOR;
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_texcoord1 : TEXCOORD1;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 positionCS : SV_POSITION;
				float4 ase_color : COLOR;
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_texcoord1 : TEXCOORD1;
				float4 ase_texcoord2 : TEXCOORD2;
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
				o.ase_texcoord2 = screenPos;
				
				o.ase_color = v.ase_color;
				o.ase_texcoord = v.ase_texcoord;
				o.ase_texcoord1 = v.ase_texcoord1;
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
				float2 uv_DetailNoise = IN.ase_texcoord.xy * _DetailNoise_ST.xy + _DetailNoise_ST.zw;
				float2 panner209 = ( 1.0 * _Time.y * _DetailNoisePanning + uv_DetailNoise);
				float4 tex2DNode211 = tex2D( _DetailNoise, panner209 );
				float4 break17_g136 = tex2DNode211;
				float4 appendResult18_g136 = (float4(break17_g136.x , break17_g136.y , break17_g136.z , break17_g136.w));
				float4 clampResult19_g136 = clamp( ( appendResult18_g136 * _DetailDistortionChannel ) , float4( 0,0,0,0 ) , float4( 1,1,1,1 ) );
				float4 break2_g136 = clampResult19_g136;
				float clampResult20_g136 = clamp( ( break2_g136.x + break2_g136.y + break2_g136.z + break2_g136.w ) , 0.0 , 1.0 );
				float3 temp_cast_1 = (clampResult20_g136).xxx;
				float3 desaturateInitialColor213 = temp_cast_1;
				float desaturateDot213 = dot( desaturateInitialColor213, float3( 0.299, 0.587, 0.114 ));
				float3 desaturateVar213 = lerp( desaturateInitialColor213, desaturateDot213.xxx, 1.0 );
				float3 DistortionNoise214 = desaturateVar213;
				float4 texCoord192 = IN.ase_texcoord1;
				texCoord192.xy = IN.ase_texcoord1.xy * float2( 1,1 ) + float2( 0,0 );
				float2 appendResult194 = (float2(texCoord192.z , 0.0));
				float2 appendResult195 = (float2(0.0 , 0.0));
				float2 LocalUVOffset197 = ( appendResult194 + appendResult195 );
				float2 texCoord200 = IN.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float3 UVFlipbookInput205 = ( ( DistortionNoise214 * _DistortionIntensity ) + float3( ( LocalUVOffset197 + texCoord200 ) ,  0.0 ) );
				float2 break135_g140 = UVFlipbookInput205.xy;
				float2 appendResult206_g140 = (float2(frac( break135_g140.x ) , frac( break135_g140.y )));
				float temp_output_4_0_g140 = _FlipbooksColumsRows.x;
				float temp_output_5_0_g140 = _FlipbooksColumsRows.y;
				float2 appendResult116_g140 = (float2(temp_output_4_0_g140 , temp_output_5_0_g140));
				float temp_output_122_0_g140 = ( temp_output_4_0_g140 * temp_output_5_0_g140 );
				float2 appendResult175_g140 = (float2(temp_output_122_0_g140 , temp_output_5_0_g140));
				float Columns213_g140 = temp_output_4_0_g140;
				float Rows212_g140 = temp_output_5_0_g140;
				float temp_output_133_0_g140 = ( fmod( _TimeParameters.x , ( Columns213_g140 * Rows212_g140 ) ) * 0.0 );
				float4 texCoord218 = IN.ase_texcoord1;
				texCoord218.xy = IN.ase_texcoord1.xy * float2( 1,1 ) + float2( 0,0 );
				float clampResult129_g140 = clamp( texCoord218.x , 1E-05 , ( temp_output_122_0_g140 - 1.0 ) );
				float temp_output_185_0_g140 = frac( ( ( temp_output_133_0_g140 + ( clampResult129_g140 + 1E-05 ) ) / temp_output_122_0_g140 ) );
				float2 appendResult186_g140 = (float2(temp_output_185_0_g140 , ( 1.0 - temp_output_185_0_g140 )));
				float2 temp_output_203_0_g140 = ( ( appendResult206_g140 / appendResult116_g140 ) + ( floor( ( appendResult175_g140 * appendResult186_g140 ) ) / appendResult116_g140 ) );
				float2 temp_output_225_0 = temp_output_203_0_g140;
				float2 panner177 = ( 1.0 * _Time.y * _MainTexturePanning + temp_output_225_0);
				float4 break17_g144 = tex2D( _MainTex, panner177 );
				float4 appendResult18_g144 = (float4(break17_g144.x , break17_g144.y , break17_g144.z , break17_g144.w));
				float4 clampResult19_g144 = clamp( ( appendResult18_g144 * _MainTextureChannel ) , float4( 0,0,0,0 ) , float4( 1,1,1,1 ) );
				float4 break2_g144 = clampResult19_g144;
				float clampResult20_g144 = clamp( ( break2_g144.x + break2_g144.y + break2_g144.z + break2_g144.w ) , 0.0 , 1.0 );
				float MainTexInfo180 = clampResult20_g144;
				float3 temp_cast_5 = (MainTexInfo180).xxx;
				float3 desaturateInitialColor175 = temp_cast_5;
				float desaturateDot175 = dot( desaturateInitialColor175, float3( 0.299, 0.587, 0.114 ));
				float3 desaturateVar175 = lerp( desaturateInitialColor175, desaturateDot175.xxx, _Desaturate );
				float4 break243 = ( _DetailAdditiveChannel * tex2DNode211 );
				float4 appendResult237 = (float4(break243.x , break243.y , break243.z , break243.w));
				float3 desaturateInitialColor241 = appendResult237.xyz;
				float desaturateDot241 = dot( desaturateInitialColor241, float3( 0.299, 0.587, 0.114 ));
				float3 desaturateVar241 = lerp( desaturateInitialColor241, desaturateDot241.xxx, 1.0 );
				float3 AdditiveNoise245 = desaturateVar241;
				float4 texCoord184 = IN.ase_texcoord;
				texCoord184.xy = IN.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float4 break17_g145 = tex2DNode211;
				float4 appendResult18_g145 = (float4(break17_g145.x , break17_g145.y , break17_g145.z , break17_g145.w));
				float4 clampResult19_g145 = clamp( ( appendResult18_g145 * _DetailMultiplyChannel ) , float4( 0,0,0,0 ) , float4( 1,1,1,1 ) );
				float4 break2_g145 = clampResult19_g145;
				float clampResult20_g145 = clamp( ( break2_g145.x + break2_g145.y + break2_g145.z + break2_g145.w ) , 0.0 , 1.0 );
				float ifLocalVar238 = 0;
				if( ( _DetailMultiplyChannel.x + _DetailMultiplyChannel.y + _DetailMultiplyChannel.z + _DetailMultiplyChannel.w ) <= 0.0 )
				ifLocalVar238 = 1.0;
				else
				ifLocalVar238 = clampResult20_g145;
				float3 temp_cast_10 = (ifLocalVar238).xxx;
				float3 desaturateInitialColor236 = temp_cast_10;
				float desaturateDot236 = dot( desaturateInitialColor236, float3( 0.299, 0.587, 0.114 ));
				float3 desaturateVar236 = lerp( desaturateInitialColor236, desaturateDot236.xxx, 1.0 );
				float3 MultiplyNoise246 = desaturateVar236;
				float2 uv_AlphaOverride = IN.ase_texcoord.xy * _AlphaOverride_ST.xy + _AlphaOverride_ST.zw;
				float2 break135_g139 = ( LocalUVOffset197 + uv_AlphaOverride );
				float2 appendResult206_g139 = (float2(frac( break135_g139.x ) , frac( break135_g139.y )));
				float temp_output_4_0_g139 = _FlipbooksColumsRows.x;
				float temp_output_5_0_g139 = _FlipbooksColumsRows.y;
				float2 appendResult116_g139 = (float2(temp_output_4_0_g139 , temp_output_5_0_g139));
				float temp_output_122_0_g139 = ( temp_output_4_0_g139 * temp_output_5_0_g139 );
				float2 appendResult175_g139 = (float2(temp_output_122_0_g139 , temp_output_5_0_g139));
				float Columns213_g139 = temp_output_4_0_g139;
				float Rows212_g139 = temp_output_5_0_g139;
				float temp_output_133_0_g139 = ( fmod( _TimeParameters.x , ( Columns213_g139 * Rows212_g139 ) ) * 0.0 );
				float clampResult129_g139 = clamp( texCoord218.x , 1E-05 , ( temp_output_122_0_g139 - 1.0 ) );
				float temp_output_185_0_g139 = frac( ( ( temp_output_133_0_g139 + ( clampResult129_g139 + 1E-05 ) ) / temp_output_122_0_g139 ) );
				float2 appendResult186_g139 = (float2(temp_output_185_0_g139 , ( 1.0 - temp_output_185_0_g139 )));
				float2 temp_output_203_0_g139 = ( ( appendResult206_g139 / appendResult116_g139 ) + ( floor( ( appendResult175_g139 * appendResult186_g139 ) ) / appendResult116_g139 ) );
				float2 panner227 = ( 1.0 * _Time.y * _AlphaOverridePanning + temp_output_203_0_g139);
				float4 break2_g141 = ( tex2D( _AlphaOverride, panner227 ) * _AlphaOverrideChannel );
				float AlphaOverride234 = saturate( ( break2_g141.x + break2_g141.y + break2_g141.z + break2_g141.w ) );
				float2 panner226 = ( 1.0 * _Time.y * _MainAlphaPanning + temp_output_225_0);
				float4 break2_g142 = ( tex2D( _MainTex, panner226 ) * _MainAlphaChannel );
				float MainAlpha233 = saturate( ( break2_g142.x + break2_g142.y + break2_g142.z + break2_g142.w ) );
				float temp_output_169_0 = ( AlphaOverride234 * MainAlpha233 );
				float4 texCoord171 = IN.ase_texcoord;
				texCoord171.xy = IN.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g146 = ( texCoord171.w - ( 1.0 - temp_output_169_0 ) );
				float temp_output_188_0 = ( IN.ase_color.a * temp_output_169_0 * saturate( saturate( ( temp_output_3_0_g146 / fwidth( temp_output_3_0_g146 ) ) ) ) );
				float4 screenPos = IN.ase_texcoord2;
				float4 ase_screenPosNorm = screenPos / screenPos.w;
				ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
				float screenDepth251 = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH( ase_screenPosNorm.xy ),_ZBufferParams);
				float distanceDepth251 = abs( ( screenDepth251 - LinearEyeDepth( ase_screenPosNorm.z,_ZBufferParams ) ) / ( _SoftFadeFactor ) );
				#ifdef _USESOFTALPHA_ON
				float staticSwitch249 = ( temp_output_188_0 * saturate( distanceDepth251 ) );
				#else
				float staticSwitch249 = temp_output_188_0;
				#endif
				float4 break265 = ( ( IN.ase_color * float4( ( desaturateVar175 + AdditiveNoise245 ) , 0.0 ) * ( texCoord184.z + 1.0 ) * float4( MultiplyNoise246 , 0.0 ) ) * staticSwitch249 );
				float4 appendResult267 = (float4(break265.r , break265.g , break265.b , staticSwitch249));
				
				float4 Color = appendResult267;

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
			#define REQUIRE_DEPTH_TEXTURE 1


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
        	#pragma shader_feature_local _USESOFTALPHA_ON


			sampler2D _MainTex;
			sampler2D _DetailNoise;
			sampler2D _AlphaOverride;
			CBUFFER_START( UnityPerMaterial )
			float4 _DetailNoise_ST;
			float4 _DetailDistortionChannel;
			float4 _MainTextureChannel;
			float4 _DetailAdditiveChannel;
			float4 _DetailMultiplyChannel;
			float4 _AlphaOverride_ST;
			float4 _AlphaOverrideChannel;
			float4 _MainAlphaChannel;
			float2 _MainTexturePanning;
			float2 _DetailNoisePanning;
			float2 _FlipbooksColumsRows;
			float2 _AlphaOverridePanning;
			float2 _MainAlphaPanning;
			float _DistortionIntensity;
			float _Desaturate;
			float _SoftFadeFactor;
			CBUFFER_END


            struct VertexInput
			{
				float3 positionOS : POSITION;
				float3 normal : NORMAL;
				float4 tangent : TANGENT;
				float4 ase_color : COLOR;
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_texcoord1 : TEXCOORD1;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 positionCS : SV_POSITION;
				float4 ase_color : COLOR;
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_texcoord1 : TEXCOORD1;
				float4 ase_texcoord2 : TEXCOORD2;
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
				o.ase_texcoord2 = screenPos;
				
				o.ase_color = v.ase_color;
				o.ase_texcoord = v.ase_texcoord;
				o.ase_texcoord1 = v.ase_texcoord1;
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
				float2 uv_DetailNoise = IN.ase_texcoord.xy * _DetailNoise_ST.xy + _DetailNoise_ST.zw;
				float2 panner209 = ( 1.0 * _Time.y * _DetailNoisePanning + uv_DetailNoise);
				float4 tex2DNode211 = tex2D( _DetailNoise, panner209 );
				float4 break17_g136 = tex2DNode211;
				float4 appendResult18_g136 = (float4(break17_g136.x , break17_g136.y , break17_g136.z , break17_g136.w));
				float4 clampResult19_g136 = clamp( ( appendResult18_g136 * _DetailDistortionChannel ) , float4( 0,0,0,0 ) , float4( 1,1,1,1 ) );
				float4 break2_g136 = clampResult19_g136;
				float clampResult20_g136 = clamp( ( break2_g136.x + break2_g136.y + break2_g136.z + break2_g136.w ) , 0.0 , 1.0 );
				float3 temp_cast_1 = (clampResult20_g136).xxx;
				float3 desaturateInitialColor213 = temp_cast_1;
				float desaturateDot213 = dot( desaturateInitialColor213, float3( 0.299, 0.587, 0.114 ));
				float3 desaturateVar213 = lerp( desaturateInitialColor213, desaturateDot213.xxx, 1.0 );
				float3 DistortionNoise214 = desaturateVar213;
				float4 texCoord192 = IN.ase_texcoord1;
				texCoord192.xy = IN.ase_texcoord1.xy * float2( 1,1 ) + float2( 0,0 );
				float2 appendResult194 = (float2(texCoord192.z , 0.0));
				float2 appendResult195 = (float2(0.0 , 0.0));
				float2 LocalUVOffset197 = ( appendResult194 + appendResult195 );
				float2 texCoord200 = IN.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float3 UVFlipbookInput205 = ( ( DistortionNoise214 * _DistortionIntensity ) + float3( ( LocalUVOffset197 + texCoord200 ) ,  0.0 ) );
				float2 break135_g140 = UVFlipbookInput205.xy;
				float2 appendResult206_g140 = (float2(frac( break135_g140.x ) , frac( break135_g140.y )));
				float temp_output_4_0_g140 = _FlipbooksColumsRows.x;
				float temp_output_5_0_g140 = _FlipbooksColumsRows.y;
				float2 appendResult116_g140 = (float2(temp_output_4_0_g140 , temp_output_5_0_g140));
				float temp_output_122_0_g140 = ( temp_output_4_0_g140 * temp_output_5_0_g140 );
				float2 appendResult175_g140 = (float2(temp_output_122_0_g140 , temp_output_5_0_g140));
				float Columns213_g140 = temp_output_4_0_g140;
				float Rows212_g140 = temp_output_5_0_g140;
				float temp_output_133_0_g140 = ( fmod( _TimeParameters.x , ( Columns213_g140 * Rows212_g140 ) ) * 0.0 );
				float4 texCoord218 = IN.ase_texcoord1;
				texCoord218.xy = IN.ase_texcoord1.xy * float2( 1,1 ) + float2( 0,0 );
				float clampResult129_g140 = clamp( texCoord218.x , 1E-05 , ( temp_output_122_0_g140 - 1.0 ) );
				float temp_output_185_0_g140 = frac( ( ( temp_output_133_0_g140 + ( clampResult129_g140 + 1E-05 ) ) / temp_output_122_0_g140 ) );
				float2 appendResult186_g140 = (float2(temp_output_185_0_g140 , ( 1.0 - temp_output_185_0_g140 )));
				float2 temp_output_203_0_g140 = ( ( appendResult206_g140 / appendResult116_g140 ) + ( floor( ( appendResult175_g140 * appendResult186_g140 ) ) / appendResult116_g140 ) );
				float2 temp_output_225_0 = temp_output_203_0_g140;
				float2 panner177 = ( 1.0 * _Time.y * _MainTexturePanning + temp_output_225_0);
				float4 break17_g144 = tex2D( _MainTex, panner177 );
				float4 appendResult18_g144 = (float4(break17_g144.x , break17_g144.y , break17_g144.z , break17_g144.w));
				float4 clampResult19_g144 = clamp( ( appendResult18_g144 * _MainTextureChannel ) , float4( 0,0,0,0 ) , float4( 1,1,1,1 ) );
				float4 break2_g144 = clampResult19_g144;
				float clampResult20_g144 = clamp( ( break2_g144.x + break2_g144.y + break2_g144.z + break2_g144.w ) , 0.0 , 1.0 );
				float MainTexInfo180 = clampResult20_g144;
				float3 temp_cast_5 = (MainTexInfo180).xxx;
				float3 desaturateInitialColor175 = temp_cast_5;
				float desaturateDot175 = dot( desaturateInitialColor175, float3( 0.299, 0.587, 0.114 ));
				float3 desaturateVar175 = lerp( desaturateInitialColor175, desaturateDot175.xxx, _Desaturate );
				float4 break243 = ( _DetailAdditiveChannel * tex2DNode211 );
				float4 appendResult237 = (float4(break243.x , break243.y , break243.z , break243.w));
				float3 desaturateInitialColor241 = appendResult237.xyz;
				float desaturateDot241 = dot( desaturateInitialColor241, float3( 0.299, 0.587, 0.114 ));
				float3 desaturateVar241 = lerp( desaturateInitialColor241, desaturateDot241.xxx, 1.0 );
				float3 AdditiveNoise245 = desaturateVar241;
				float4 texCoord184 = IN.ase_texcoord;
				texCoord184.xy = IN.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float4 break17_g145 = tex2DNode211;
				float4 appendResult18_g145 = (float4(break17_g145.x , break17_g145.y , break17_g145.z , break17_g145.w));
				float4 clampResult19_g145 = clamp( ( appendResult18_g145 * _DetailMultiplyChannel ) , float4( 0,0,0,0 ) , float4( 1,1,1,1 ) );
				float4 break2_g145 = clampResult19_g145;
				float clampResult20_g145 = clamp( ( break2_g145.x + break2_g145.y + break2_g145.z + break2_g145.w ) , 0.0 , 1.0 );
				float ifLocalVar238 = 0;
				if( ( _DetailMultiplyChannel.x + _DetailMultiplyChannel.y + _DetailMultiplyChannel.z + _DetailMultiplyChannel.w ) <= 0.0 )
				ifLocalVar238 = 1.0;
				else
				ifLocalVar238 = clampResult20_g145;
				float3 temp_cast_10 = (ifLocalVar238).xxx;
				float3 desaturateInitialColor236 = temp_cast_10;
				float desaturateDot236 = dot( desaturateInitialColor236, float3( 0.299, 0.587, 0.114 ));
				float3 desaturateVar236 = lerp( desaturateInitialColor236, desaturateDot236.xxx, 1.0 );
				float3 MultiplyNoise246 = desaturateVar236;
				float2 uv_AlphaOverride = IN.ase_texcoord.xy * _AlphaOverride_ST.xy + _AlphaOverride_ST.zw;
				float2 break135_g139 = ( LocalUVOffset197 + uv_AlphaOverride );
				float2 appendResult206_g139 = (float2(frac( break135_g139.x ) , frac( break135_g139.y )));
				float temp_output_4_0_g139 = _FlipbooksColumsRows.x;
				float temp_output_5_0_g139 = _FlipbooksColumsRows.y;
				float2 appendResult116_g139 = (float2(temp_output_4_0_g139 , temp_output_5_0_g139));
				float temp_output_122_0_g139 = ( temp_output_4_0_g139 * temp_output_5_0_g139 );
				float2 appendResult175_g139 = (float2(temp_output_122_0_g139 , temp_output_5_0_g139));
				float Columns213_g139 = temp_output_4_0_g139;
				float Rows212_g139 = temp_output_5_0_g139;
				float temp_output_133_0_g139 = ( fmod( _TimeParameters.x , ( Columns213_g139 * Rows212_g139 ) ) * 0.0 );
				float clampResult129_g139 = clamp( texCoord218.x , 1E-05 , ( temp_output_122_0_g139 - 1.0 ) );
				float temp_output_185_0_g139 = frac( ( ( temp_output_133_0_g139 + ( clampResult129_g139 + 1E-05 ) ) / temp_output_122_0_g139 ) );
				float2 appendResult186_g139 = (float2(temp_output_185_0_g139 , ( 1.0 - temp_output_185_0_g139 )));
				float2 temp_output_203_0_g139 = ( ( appendResult206_g139 / appendResult116_g139 ) + ( floor( ( appendResult175_g139 * appendResult186_g139 ) ) / appendResult116_g139 ) );
				float2 panner227 = ( 1.0 * _Time.y * _AlphaOverridePanning + temp_output_203_0_g139);
				float4 break2_g141 = ( tex2D( _AlphaOverride, panner227 ) * _AlphaOverrideChannel );
				float AlphaOverride234 = saturate( ( break2_g141.x + break2_g141.y + break2_g141.z + break2_g141.w ) );
				float2 panner226 = ( 1.0 * _Time.y * _MainAlphaPanning + temp_output_225_0);
				float4 break2_g142 = ( tex2D( _MainTex, panner226 ) * _MainAlphaChannel );
				float MainAlpha233 = saturate( ( break2_g142.x + break2_g142.y + break2_g142.z + break2_g142.w ) );
				float temp_output_169_0 = ( AlphaOverride234 * MainAlpha233 );
				float4 texCoord171 = IN.ase_texcoord;
				texCoord171.xy = IN.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_3_0_g146 = ( texCoord171.w - ( 1.0 - temp_output_169_0 ) );
				float temp_output_188_0 = ( IN.ase_color.a * temp_output_169_0 * saturate( saturate( ( temp_output_3_0_g146 / fwidth( temp_output_3_0_g146 ) ) ) ) );
				float4 screenPos = IN.ase_texcoord2;
				float4 ase_screenPosNorm = screenPos / screenPos.w;
				ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
				float screenDepth251 = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH( ase_screenPosNorm.xy ),_ZBufferParams);
				float distanceDepth251 = abs( ( screenDepth251 - LinearEyeDepth( ase_screenPosNorm.z,_ZBufferParams ) ) / ( _SoftFadeFactor ) );
				#ifdef _USESOFTALPHA_ON
				float staticSwitch249 = ( temp_output_188_0 * saturate( distanceDepth251 ) );
				#else
				float staticSwitch249 = temp_output_188_0;
				#endif
				float4 break265 = ( ( IN.ase_color * float4( ( desaturateVar175 + AdditiveNoise245 ) , 0.0 ) * ( texCoord184.z + 1.0 ) * float4( MultiplyNoise246 , 0.0 ) ) * staticSwitch249 );
				float4 appendResult267 = (float4(break265.r , break265.g , break265.b , staticSwitch249));
				
				float4 Color = appendResult267;
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
Node;AmplifyShaderEditor.CommentaryNode;158;-999.8004,-2121.479;Inherit;False;1884.647;1001.187;Extra Noise Setup;22;247;246;245;244;243;242;241;240;239;238;237;236;235;214;213;212;211;210;209;208;207;206;;1,1,1,1;0;0
Node;AmplifyShaderEditor.TexturePropertyNode;206;-960.6143,-1878.96;Inherit;True;Property;_DetailNoise;Detail Noise;10;0;Create;False;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.Vector2Node;207;-706.5922,-1589.672;Inherit;False;Property;_DetailNoisePanning;Detail Noise Panning;11;0;Create;False;0;0;0;False;0;False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.TextureCoordinatesNode;208;-711.1732,-1705.759;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;159;941.6688,-1589.481;Inherit;False;869.2021;446.9999;UV Offset Controlled by custom vertex stream;6;197;196;195;194;193;192;;0.3699214,0.2971698,1,1;0;0
Node;AmplifyShaderEditor.PannerNode;209;-473.1751,-1630.759;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;192;973.6688,-1525.481;Inherit;False;1;-1;4;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector4Node;210;-262.3925,-1667.27;Inherit;False;Property;_DetailDistortionChannel;Detail Distortion Channel;12;0;Create;False;0;0;0;False;0;False;1,0,0,0;1,0,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;193;989.6688,-1285.482;Inherit;False;Constant;_InitialOffset1;Initial Offset;16;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;211;-286.3775,-1875.46;Inherit;True;Property;_TextureSample4;Texture Sample 3;4;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;194;1181.669,-1461.482;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.FunctionNode;212;16.89372,-1865.296;Inherit;False;Channel Picker;-1;;136;dc5f4cb24a8bdf448b40a1ec5866280e;0;2;5;FLOAT4;1,0,0,0;False;7;FLOAT4;0,0,0,1;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;195;1261.669,-1285.482;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;196;1453.669,-1317.482;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;1,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DesaturateOpNode;213;44.27821,-1778.763;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;214;26.39369,-1691.096;Inherit;False;DistortionNoise;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;197;1613.669,-1317.482;Inherit;False;LocalUVOffset;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;160;949.5999,-2249.169;Inherit;False;853.4072;636.7309;Set UV Modifiers For Main Tex;8;205;204;203;202;201;200;199;198;;1,0.8279877,0,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;198;1039.233,-1904.855;Inherit;False;197;LocalUVOffset;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;200;1006.238,-1776.438;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;201;1003.6,-2199.169;Inherit;False;214;DistortionNoise;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;199;999.5999,-2037.169;Inherit;False;Property;_DistortionIntensity;Distortion Intensity;13;0;Create;False;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;202;1265.09,-1927.62;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;203;1225.602,-2124.17;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CommentaryNode;161;-3057.841,-2413.333;Inherit;False;1894.068;530.1917;Alpha Override;12;234;232;230;228;227;224;223;222;221;217;216;215;;0,0.5461459,1,1;0;0
Node;AmplifyShaderEditor.SimpleAddOpNode;204;1402.602,-1991.169;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT2;0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TexturePropertyNode;215;-3027.099,-2354.74;Inherit;True;Property;_AlphaOverride;Alpha Override;6;0;Create;False;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.TextureCoordinatesNode;216;-2765.5,-2199.046;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;162;-2694.571,-1378.17;Inherit;False;1576.333;998.0396;Main Texture Set Vars;16;233;231;229;226;225;220;182;181;180;179;178;177;166;165;164;163;;0,0.5461459,1,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;217;-2739.098,-2274.74;Inherit;False;197;LocalUVOffset;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;205;1569.008,-1935.55;Inherit;False;UVFlipbookInput;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;220;-2670.452,-1310.065;Inherit;False;205;UVFlipbookInput;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;222;-2547.099,-2274.74;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector2Node;219;-2684.954,-1681.634;Inherit;False;Property;_FlipbooksColumsRows;Flipbooks Colums & Rows;9;0;Create;False;0;0;0;False;0;False;1,1;1,1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.TextureCoordinatesNode;218;-2658.401,-1844.135;Inherit;False;1;-1;4;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;224;-2421.211,-2274.714;Inherit;False;Flipbook;-1;;139;53c2488c220f6564ca6c90721ee16673;3,68,0,217,0,244,0;11;51;SAMPLER2D;0.0;False;167;SAMPLERSTATE;0;False;13;FLOAT2;0,0;False;24;FLOAT;0;False;210;FLOAT;4;False;4;FLOAT;3;False;5;FLOAT;3;False;130;FLOAT;0;False;2;FLOAT;0;False;55;FLOAT;0;False;70;FLOAT;0;False;5;COLOR;53;FLOAT2;0;FLOAT;47;FLOAT;48;FLOAT;218
Node;AmplifyShaderEditor.Vector2Node;164;-2112.326,-1163.986;Inherit;False;Property;_MainAlphaPanning;Main Alpha Panning;5;0;Create;False;0;0;0;False;0;False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.Vector2Node;223;-2435.979,-2039.085;Inherit;False;Property;_AlphaOverridePanning;Alpha Override Panning;7;0;Create;False;0;0;0;False;0;False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.FunctionNode;225;-2435.66,-1302.777;Inherit;False;Flipbook;-1;;140;53c2488c220f6564ca6c90721ee16673;3,68,0,217,0,244,0;11;51;SAMPLER2D;0.0;False;167;SAMPLERSTATE;0;False;13;FLOAT2;0,0;False;24;FLOAT;0;False;210;FLOAT;4;False;4;FLOAT;3;False;5;FLOAT;3;False;130;FLOAT;0;False;2;FLOAT;0;False;55;FLOAT;0;False;70;FLOAT;0;False;5;COLOR;53;FLOAT2;0;FLOAT;47;FLOAT;48;FLOAT;218
Node;AmplifyShaderEditor.TexturePropertyNode;165;-2144.326,-971.9857;Inherit;True;Property;_MainTex;Main Texture;0;0;Create;False;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.PannerNode;227;-2147.098,-2242.74;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PannerNode;226;-2112.326,-1275.986;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector4Node;230;-1897.099,-2152.74;Inherit;False;Property;_AlphaOverrideChannel;Alpha Override Channel;8;0;Create;False;0;0;0;False;0;False;1,0,0,0;1,0,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;228;-1955.099,-2338.74;Inherit;True;Property;_TextureSample3;Texture Sample 2;4;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector4Node;166;-1863.4,-1068.224;Inherit;False;Property;_MainAlphaChannel;Main Alpha Channel;3;0;Create;False;0;0;0;False;0;False;0,0,0,1;0,0,0,1;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;229;-1889.326,-1291.986;Inherit;True;Property;_TextureSample2;Texture Sample 1;4;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;232;-1651.099,-2338.74;Inherit;False;Channel Picker Alpha;-1;;141;e49841402b321534583d1dc019041b68;0;2;5;FLOAT4;1,0,0,0;False;7;FLOAT4;0,0,0,1;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;231;-1568.326,-1291.986;Inherit;False;Channel Picker Alpha;-1;;142;e49841402b321534583d1dc019041b68;0;2;5;FLOAT4;1,0,0,0;False;7;FLOAT4;0,0,0,1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;234;-1444.099,-2338.74;Inherit;True;AlphaOverride;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;233;-1344.326,-1291.986;Inherit;True;MainAlpha;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;178;-2128.326,-587.9854;Inherit;False;Property;_MainTexturePanning;Main Texture Panning;4;0;Create;False;0;0;0;False;0;False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.Vector4Node;242;5.107399,-2047.171;Inherit;False;Property;_DetailAdditiveChannel;Detail Additive Channel;15;0;Create;False;0;0;0;False;0;False;0,0,0,0;0,0,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;168;-332.8427,-311.5;Inherit;False;233;MainAlpha;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;167;-355.2169,-387.1024;Inherit;False;234;AlphaOverride;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;177;-2112.326,-699.9854;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;240;280.4949,-1897.861;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;169;-140.8424,-407.5;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector4Node;244;-339.9926,-1436.573;Inherit;False;Property;_DetailMultiplyChannel;Detail Multiply Channel;14;0;Create;False;0;0;0;False;0;False;0,0,0,0;0,0,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector4Node;182;-1895.393,-607.8138;Inherit;False;Property;_MainTextureChannel;Main Texture Channel;2;0;Create;False;0;0;0;False;0;False;1,1,1,0;1,1,1,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;181;-1904.326,-827.9854;Inherit;True;Property;_TextureSample1;Texture Sample 0;0;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.BreakToComponentsNode;243;403.2616,-1902.327;Inherit;False;FLOAT4;1;0;FLOAT4;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.OneMinusNode;170;41.97596,-291.4886;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;171;-18.77578,-216.675;Inherit;False;0;-1;4;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;239;-90.65478,-1406.945;Inherit;False;4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;237;530.0615,-1905.428;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.FunctionNode;179;-1552.326,-635.9854;Inherit;False;Channel Picker;-1;;144;dc5f4cb24a8bdf448b40a1ec5866280e;0;2;5;FLOAT4;1,0,0,0;False;7;FLOAT4;0,0,0,1;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;247;37.09401,-1359.597;Inherit;False;Channel Picker;-1;;145;dc5f4cb24a8bdf448b40a1ec5866280e;0;2;5;FLOAT4;1,0,0,0;False;7;FLOAT4;0,0,0,1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;235;106.974,-1223.582;Inherit;False;Constant;_Float1;Float 0;15;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;172;218.8035,-224.0667;Inherit;False;Step Antialiasing;-1;;146;2a825e80dfb3290468194f83380797bd;0;2;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;252;18.66154,186.41;Inherit;False;Property;_SoftFadeFactor;SoftFadeFactor;17;0;Create;False;0;0;0;False;0;False;0.1;0.1;0.1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;180;-1344.326,-635.9854;Inherit;True;MainTexInfo;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ConditionalIfNode;238;268.4739,-1408.582;Inherit;False;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DesaturateOpNode;241;658.2786,-1900.865;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SaturateNode;173;399.2592,-224.9552;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DepthFade;251;301.6616,115.41;Inherit;False;True;False;True;2;1;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;174;564.4019,-428.0732;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DesaturateOpNode;236;426.1782,-1408.064;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;187;-77.69921,-943.9547;Inherit;False;180;MainTexInfo;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;245;655.1942,-1812.597;Inherit;False;AdditiveNoise;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;176;-131.8308,-787.2311;Inherit;False;Property;_Desaturate;Desaturate?;1;0;Create;False;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;188;584.1821,-268.765;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;250;557.6616,110.41;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;184;374.8037,-748.0399;Inherit;False;0;-1;4;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;246;583.5939,-1409.197;Inherit;False;MultiplyNoise;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DesaturateOpNode;175;-72.77139,-873.7276;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;189;123.0929,-846.6474;Inherit;False;245;AdditiveNoise;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;248;780.0134,-89.69116;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;185;426.1795,-1043.084;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;190;466.4144,-871.5717;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;183;622.5521,-760.8052;Inherit;False;246;MultiplyNoise;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;186;419.0553,-585.8185;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;191;648.5884,-901.9236;Inherit;False;4;4;0;COLOR;1,0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;3;FLOAT3;0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StaticSwitch;249;844.0134,-281.6912;Inherit;False;Property;_UseSoftAlpha;UseSoftAlpha;16;0;Create;False;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;264;1067.518,-689.8449;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.BreakToComponentsNode;265;1249.036,-564.6475;Inherit;True;COLOR;1;0;COLOR;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.IntNode;221;-2422.593,-2113.9;Inherit;False;Constant;_Int;Int ;9;0;Create;True;0;0;0;False;0;False;0;0;False;0;1;INT;0
Node;AmplifyShaderEditor.IntNode;163;-2433.973,-1142.627;Inherit;False;Constant;_Int0;Int 0;9;0;Create;True;0;0;0;False;0;False;0;0;False;0;1;INT;0
Node;AmplifyShaderEditor.DynamicAppendNode;267;1591.036,-432.6476;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;268;1823.302,-625.1548;Float;False;True;-1;2;ASEMaterialInspector;0;15;Piloto Studio/Additive Uber Shader;cf964e524c8e69742b1d21fbe2ebcc4a;True;Sprite Unlit;0;0;Sprite Unlit;4;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Transparent=RenderType;Queue=Transparent=Queue=0;True;0;True;12;all;0;False;True;2;5;False;;10;False;;3;1;False;;10;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;2;False;;True;3;False;;True;True;0;False;;0;False;;True;1;LightMode=Universal2D;False;False;0;Hidden/InternalErrorShader;0;0;Standard;3;Vertex Position;1;0;Debug Display;0;0;External Alpha;0;0;0;4;True;True;True;True;False;;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;269;1823.302,-625.1548;Float;False;False;-1;2;ASEMaterialInspector;0;1;New Amplify Shader;cf964e524c8e69742b1d21fbe2ebcc4a;True;Sprite Unlit Forward;0;1;Sprite Unlit Forward;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Transparent=RenderType;Queue=Transparent=Queue=0;True;0;True;12;all;0;False;True;2;5;False;;10;False;;3;1;False;;10;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;2;False;;True;3;False;;True;True;0;False;;0;False;;True;1;LightMode=UniversalForward;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;270;1823.302,-625.1548;Float;False;False;-1;2;ASEMaterialInspector;0;1;New Amplify Shader;cf964e524c8e69742b1d21fbe2ebcc4a;True;SceneSelectionPass;0;2;SceneSelectionPass;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Transparent=RenderType;Queue=Transparent=Queue=0;True;0;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=SceneSelectionPass;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;271;1823.302,-625.1548;Float;False;False;-1;2;ASEMaterialInspector;0;1;New Amplify Shader;cf964e524c8e69742b1d21fbe2ebcc4a;True;ScenePickingPass;0;3;ScenePickingPass;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Transparent=RenderType;Queue=Transparent=Queue=0;True;0;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=Picking;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
WireConnection;208;2;206;0
WireConnection;209;0;208;0
WireConnection;209;2;207;0
WireConnection;211;0;206;0
WireConnection;211;1;209;0
WireConnection;194;0;192;3
WireConnection;212;5;211;0
WireConnection;212;7;210;0
WireConnection;195;0;193;0
WireConnection;196;0;194;0
WireConnection;196;1;195;0
WireConnection;213;0;212;0
WireConnection;214;0;213;0
WireConnection;197;0;196;0
WireConnection;202;0;198;0
WireConnection;202;1;200;0
WireConnection;203;0;201;0
WireConnection;203;1;199;0
WireConnection;204;0;203;0
WireConnection;204;1;202;0
WireConnection;216;2;215;0
WireConnection;205;0;204;0
WireConnection;222;0;217;0
WireConnection;222;1;216;0
WireConnection;224;13;222;0
WireConnection;224;24;218;1
WireConnection;224;4;219;1
WireConnection;224;5;219;2
WireConnection;225;13;220;0
WireConnection;225;24;218;1
WireConnection;225;4;219;1
WireConnection;225;5;219;2
WireConnection;227;0;224;0
WireConnection;227;2;223;0
WireConnection;226;0;225;0
WireConnection;226;2;164;0
WireConnection;228;0;215;0
WireConnection;228;1;227;0
WireConnection;229;0;165;0
WireConnection;229;1;226;0
WireConnection;232;5;228;0
WireConnection;232;7;230;0
WireConnection;231;5;229;0
WireConnection;231;7;166;0
WireConnection;234;0;232;0
WireConnection;233;0;231;0
WireConnection;177;0;225;0
WireConnection;177;2;178;0
WireConnection;240;0;242;0
WireConnection;240;1;211;0
WireConnection;169;0;167;0
WireConnection;169;1;168;0
WireConnection;181;0;165;0
WireConnection;181;1;177;0
WireConnection;243;0;240;0
WireConnection;170;0;169;0
WireConnection;239;0;244;1
WireConnection;239;1;244;2
WireConnection;239;2;244;3
WireConnection;239;3;244;4
WireConnection;237;0;243;0
WireConnection;237;1;243;1
WireConnection;237;2;243;2
WireConnection;237;3;243;3
WireConnection;179;5;181;0
WireConnection;179;7;182;0
WireConnection;247;5;211;0
WireConnection;247;7;244;0
WireConnection;172;1;170;0
WireConnection;172;2;171;4
WireConnection;180;0;179;0
WireConnection;238;0;239;0
WireConnection;238;2;247;0
WireConnection;238;3;235;0
WireConnection;238;4;235;0
WireConnection;241;0;237;0
WireConnection;173;0;172;0
WireConnection;251;0;252;0
WireConnection;236;0;238;0
WireConnection;245;0;241;0
WireConnection;188;0;174;4
WireConnection;188;1;169;0
WireConnection;188;2;173;0
WireConnection;250;0;251;0
WireConnection;246;0;236;0
WireConnection;175;0;187;0
WireConnection;175;1;176;0
WireConnection;248;0;188;0
WireConnection;248;1;250;0
WireConnection;190;0;175;0
WireConnection;190;1;189;0
WireConnection;186;0;184;3
WireConnection;191;0;185;0
WireConnection;191;1;190;0
WireConnection;191;2;186;0
WireConnection;191;3;183;0
WireConnection;249;1;188;0
WireConnection;249;0;248;0
WireConnection;264;0;191;0
WireConnection;264;1;249;0
WireConnection;265;0;264;0
WireConnection;267;0;265;0
WireConnection;267;1;265;1
WireConnection;267;2;265;2
WireConnection;267;3;249;0
WireConnection;268;1;267;0
ASEEND*/
//CHKSM=E24D8B2C28FF1EEDD1D8D5349AB2EA4D2CB4DE8B