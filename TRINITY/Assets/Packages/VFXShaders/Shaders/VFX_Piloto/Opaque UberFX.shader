// Made with Amplify Shader Editor v1.9.6.3
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Piloto Studio/Opaque UberFX"
{
	Properties
	{
		[Enum(UnityEngine.Rendering.BlendMode)]_SourceBlendRGB("Blend Mode", Float) = 10
		_AlphaClip("Alpha Clip", Range( 0 , 1)) = 0
		[Enum(UnityEngine.Rendering.CullMode)]_Culling("Culling", Float) = 1
		_MainTex("Main Texture", 2D) = "white" {}
		_MainTextureChannel("Main Texture Channel", Vector) = (1,1,1,0)
		_MainAlphaChannel("Main Alpha Channel", Vector) = (0,0,0,1)
		_MainTexturePanning("Main Texture Panning ", Vector) = (0,0,0,0)
		_Desaturate("Desaturate? ", Range( 0 , 1)) = 0
		[Toggle(_USESOFTALPHA_ON)] _UseSoftAlpha("Use Soft Particles?", Float) = 0
		_SoftFadeFactor("Soft Fade Factor", Range( 0.1 , 1)) = 0.1
		[Toggle(_USEALPHAOVERRIDE_ON)] _UseAlphaOverride("Use Alpha Override", Float) = 0
		_AlphaOverride("Alpha Override", 2D) = "white" {}
		_AlphaOverrideChannel("Alpha Override Channel", Vector) = (0,0,0,1)
		_AlphaOverridePanning("Alpha Override Panning", Vector) = (0,0,0,0)
		_DetailNoise("Detail Noise", 2D) = "white" {}
		_DetailNoisePanning("Detail Noise Panning", Vector) = (0,0,0,0)
		_DetailDistortionChannel("Detail Distortion Channel", Vector) = (0,0,0,0)
		_DistortionIntensity("Distortion Intensity", Range( 0 , 3)) = 2
		_DetailMultiplyChannel("Detail Multiply Channel", Vector) = (0,0,0,0)
		_MultiplyNoiseDesaturation("Multiply Noise Desaturation", Range( 0 , 1)) = 1
		_DetailAdditiveChannel("Detail Additive Channel", Vector) = (0,0,0,0)
		_DetailDisolveChannel("Detail Disolve Channel", Vector) = (1,0,0,0)
		_DetailVertexOffsetChannel("Detail Vertex Offset Channel", Vector) = (1,0,0,0)
		[Toggle(_USERAMP_ON)] _UseRamp("Use Color Ramping?", Float) = 0
		[HDR]_WhiteColor("Highs", Color) = (1,0.8950032,0,0)
		_MiddlePointPos("Middle Point Position", Range( -1 , 0.99)) = 0.5
		[HDR]_MidColor("Middles", Color) = (1,0.4447915,0,0)
		_MiddlePointPos1("Middle Point Position 2", Range( -1 , 0.99)) = 0.5
		[HDR]_LastColor("Lows", Color) = (1,0,0,0)
		[Toggle(_USEUVOFFSET_ON)] _UseUVOffset("Use UV Offset", Float) = 0
		[Toggle(_FRESNEL_ON)] _Fresnel("Fresnel", Float) = 0
		_FresnelPower("Fresnel Power", Float) = 1
		_FresnelScale("Fresnel Scale", Float) = 1
		[HDR]_FresnelColor("Fresnel Color", Color) = (1,1,1,1)
		[Toggle(_DISABLEEROSION_ON)] _DisableErosion("Disable Erosion", Float) = 0
		[Toggle(_USEPIXELATION_ON)] _UsePixelation("Use Pixelation", Float) = 0
		_Resolution("Resolution", Vector) = (64,64,0,0)

	}
	
	SubShader
	{
		Tags { "RenderType"="TransparentCutout" }
	LOD 100

		Cull [_Culling]
		CGINCLUDE
		#pragma target 3.0 
		ENDCG
		
		
		Pass
		{
			
			Name "ForwardBase"
			Tags { "LightMode"="ForwardBase" }

			CGINCLUDE
			#pragma target 3.0
			ENDCG
			Blend SrcAlpha [_SourceBlendRGB]
			AlphaToMask Off
			Cull [_Culling]
			ColorMask RGBA
			ZWrite On
			ZTest LEqual
			Offset 0 , 0
			
			CGPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fwdbase
			#ifndef UNITY_PASS_FORWARDBASE
			#define UNITY_PASS_FORWARDBASE
			#endif
			#include "UnityCG.cginc"
			#include "UnityShaderVariables.cginc"
			#define ASE_NEEDS_VERT_NORMAL
			#define ASE_NEEDS_FRAG_COLOR
			#pragma shader_feature_local _FRESNEL_ON
			#pragma shader_feature_local _USERAMP_ON
			#pragma shader_feature_local _USEPIXELATION_ON
			#pragma shader_feature_local _USEUVOFFSET_ON
			#pragma shader_feature_local _DISABLEEROSION_ON
			#pragma shader_feature_local _USESOFTALPHA_ON
			#pragma shader_feature_local _USEALPHAOVERRIDE_ON

			uniform float _SourceBlendRGB;
			uniform float _Culling;
			uniform float _AlphaClip;
			uniform sampler2D _DetailNoise;
			uniform float2 _DetailNoisePanning;
			uniform float4 _DetailNoise_ST;
			uniform float4 _DetailVertexOffsetChannel;
			uniform sampler2D _MainTex;
			uniform float2 _MainTexturePanning;
			uniform float4 _MainTex_ST;
			uniform float2 _Resolution;
			uniform float4 _DetailDistortionChannel;
			uniform float _DistortionIntensity;
			uniform float4 _MainTextureChannel;
			uniform float _Desaturate;
			uniform float4 _DetailMultiplyChannel;
			uniform float _MultiplyNoiseDesaturation;
			uniform float4 _DetailAdditiveChannel;
			uniform float4 _LastColor;
			uniform float4 _MidColor;
			uniform float _MiddlePointPos;
			uniform float _MiddlePointPos1;
			uniform float4 _WhiteColor;
			uniform float4 _DetailDisolveChannel;
			uniform sampler2D _AlphaOverride;
			uniform float2 _AlphaOverridePanning;
			uniform float4 _AlphaOverride_ST;
			uniform float4 _AlphaOverrideChannel;
			uniform float4 _MainAlphaChannel;
			UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
			uniform float4 _CameraDepthTexture_TexelSize;
			uniform float _SoftFadeFactor;
			uniform float4 _FresnelColor;
			uniform float _FresnelScale;
			uniform float _FresnelPower;


			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				float4 ase_texcoord1 : TEXCOORD1;
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_color : COLOR;
			};
			
			struct v2f
			{
				float4 pos : SV_POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
				float4 ase_color : COLOR;
				float4 ase_texcoord1 : TEXCOORD1;
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_texcoord3 : TEXCOORD3;
				float4 ase_texcoord4 : TEXCOORD4;
				float4 ase_texcoord5 : TEXCOORD5;
			};
			
			v2f vert ( appdata v )
			{
				v2f o;
				UNITY_INITIALIZE_OUTPUT(v2f,o);
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				
				float4 texCoord429 = v.ase_texcoord1;
				texCoord429.xy = v.ase_texcoord1.xy * float2( 1,1 ) + float2( 0,0 );
				float2 uv_DetailNoise = v.ase_texcoord * _DetailNoise_ST.xy + _DetailNoise_ST.zw;
				float2 panner80 = ( 1.0 * _Time.y * _DetailNoisePanning + uv_DetailNoise);
				float4 tex2DNode79 = tex2Dlod( _DetailNoise, float4( panner80, 0, 0.0) );
				float4 break17_g214 = tex2DNode79;
				float4 appendResult18_g214 = (float4(break17_g214.x , break17_g214.y , break17_g214.z , break17_g214.w));
				float4 clampResult19_g214 = clamp( ( appendResult18_g214 * _DetailVertexOffsetChannel ) , float4( 0,0,0,0 ) , float4( 1,1,1,1 ) );
				float4 break2_g214 = clampResult19_g214;
				float clampResult20_g214 = clamp( ( break2_g214.x + break2_g214.y + break2_g214.z + break2_g214.w ) , 0.0 , 1.0 );
				float VertexOffset434 = clampResult20_g214;
				
				float4 ase_clipPos = UnityObjectToClipPos(v.vertex);
				float4 screenPos = ComputeScreenPos(ase_clipPos);
				o.ase_texcoord3 = screenPos;
				float3 ase_worldPos = mul(unity_ObjectToWorld, float4( (v.vertex).xyz, 1 )).xyz;
				o.ase_texcoord4.xyz = ase_worldPos;
				float3 ase_worldNormal = UnityObjectToWorldNormal(v.normal);
				o.ase_texcoord5.xyz = ase_worldNormal;
				
				o.ase_color = v.ase_color;
				o.ase_texcoord1 = v.ase_texcoord;
				o.ase_texcoord2 = v.ase_texcoord1;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord4.w = 0;
				o.ase_texcoord5.w = 0;
				
				v.vertex.xyz += ( ( texCoord429.z * VertexOffset434 ) * v.normal );
				o.pos = UnityObjectToClipPos(v.vertex);
				#if ASE_SHADOWS
					#if UNITY_VERSION >= 560
						UNITY_TRANSFER_SHADOW( o, v.texcoord );
					#else
						TRANSFER_SHADOW( o );
					#endif
				#endif
				return o;
			}
			
			float4 frag (v2f i ) : SV_Target
			{
				float3 outColor;
				float outAlpha;

				float2 uv_MainTex = i.ase_texcoord1.xy * _MainTex_ST.xy + _MainTex_ST.zw;
				float2 break446 = uv_MainTex;
				float2 appendResult455 = (float2(( break446.x - ( fmod( break446.x , ( 1.0 / _Resolution ).x ) - ( ( 1.0 / _Resolution ).x * 0.5 ) ) ) , ( break446.y - ( fmod( break446.y , 0.0 ) - ( 0.0 * 0.5 ) ) )));
				#ifdef _USEPIXELATION_ON
				float2 staticSwitch456 = appendResult455;
				#else
				float2 staticSwitch456 = uv_MainTex;
				#endif
				float2 uv_DetailNoise = i.ase_texcoord1.xy * _DetailNoise_ST.xy + _DetailNoise_ST.zw;
				float2 panner80 = ( 1.0 * _Time.y * _DetailNoisePanning + uv_DetailNoise);
				float4 tex2DNode79 = tex2D( _DetailNoise, panner80 );
				float4 break17_g202 = tex2DNode79;
				float4 appendResult18_g202 = (float4(break17_g202.x , break17_g202.y , break17_g202.z , break17_g202.w));
				float4 clampResult19_g202 = clamp( ( appendResult18_g202 * _DetailDistortionChannel ) , float4( 0,0,0,0 ) , float4( 1,1,1,1 ) );
				float4 break2_g202 = clampResult19_g202;
				float clampResult20_g202 = clamp( ( break2_g202.x + break2_g202.y + break2_g202.z + break2_g202.w ) , 0.0 , 1.0 );
				float DistortionNoise90 = clampResult20_g202;
				float temp_output_284_0 = ( DistortionNoise90 * _DistortionIntensity );
				float2 temp_cast_1 = (temp_output_284_0).xx;
				float4 texCoord397 = i.ase_texcoord2;
				texCoord397.xy = i.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float2 appendResult400 = (float2(texCoord397.x , texCoord397.y));
				#ifdef _USEUVOFFSET_ON
				float2 staticSwitch402 = ( temp_output_284_0 + appendResult400 );
				#else
				float2 staticSwitch402 = temp_cast_1;
				#endif
				float2 UVModifiers204 = staticSwitch402;
				float2 panner22 = ( 1.0 * _Time.y * _MainTexturePanning + ( staticSwitch456 + UVModifiers204 ));
				float4 tex2DNode6 = tex2D( _MainTex, panner22 );
				float4 break376 = tex2DNode6;
				float4 break379 = _MainTextureChannel;
				float4 appendResult375 = (float4(( break376.r * break379.x ) , ( break376.g * break379.y ) , ( break376.b * break379.z ) , ( break376.a * break379.w )));
				float4 MainTexInfo25 = appendResult375;
				float3 desaturateInitialColor166 = MainTexInfo25.xyz;
				float desaturateDot166 = dot( desaturateInitialColor166, float3( 0.299, 0.587, 0.114 ));
				float3 desaturateVar166 = lerp( desaturateInitialColor166, desaturateDot166.xxx, _Desaturate );
				float4 break364 = ( _DetailMultiplyChannel * tex2DNode79 );
				float4 appendResult365 = (float4(break364.x , break364.y , break364.z , break364.w));
				float3 desaturateInitialColor362 = appendResult365.xyz;
				float desaturateDot362 = dot( desaturateInitialColor362, float3( 0.299, 0.587, 0.114 ));
				float3 desaturateVar362 = lerp( desaturateInitialColor362, desaturateDot362.xxx, _MultiplyNoiseDesaturation );
				float3 temp_cast_5 = (1.0).xxx;
				float3 temp_cast_6 = (1.0).xxx;
				float3 ifLocalVar106 = 0;
				if( ( _DetailMultiplyChannel.x + _DetailMultiplyChannel.y + _DetailMultiplyChannel.z + _DetailMultiplyChannel.w ) <= 0.0 )
				ifLocalVar106 = temp_cast_6;
				else
				ifLocalVar106 = desaturateVar362;
				float3 MultiplyNoise92 = ifLocalVar106;
				float4 break156 = ( _DetailAdditiveChannel * tex2DNode79 );
				float4 appendResult155 = (float4(break156.x , break156.y , break156.z , break156.w));
				float3 desaturateInitialColor191 = appendResult155.xyz;
				float desaturateDot191 = dot( desaturateInitialColor191, float3( 0.299, 0.587, 0.114 ));
				float3 desaturateVar191 = lerp( desaturateInitialColor191, desaturateDot191.xxx, 1.0 );
				float3 AdditiveNoise91 = desaturateVar191;
				float3 PreRamp210 = desaturateVar166;
				float3 temp_cast_10 = (_MiddlePointPos).xxx;
				float3 clampResult218 = clamp( ( PreRamp210 - temp_cast_10 ) , float3( 0,0,0 ) , float3( 1,1,1 ) );
				float temp_output_215_0 = ( 1.0 - _MiddlePointPos );
				float3 temp_cast_11 = (temp_output_215_0).xxx;
				float3 temp_output_219_0 = (float3( 0,0,0 ) + (clampResult218 - float3( 0,0,0 )) * (float3( 1,1,1 ) - float3( 0,0,0 )) / (temp_cast_11 - float3( 0,0,0 )));
				float3 temp_cast_12 = (_MiddlePointPos1).xxx;
				float3 temp_cast_13 = (temp_output_215_0).xxx;
				float4 lerpResult220 = lerp( _LastColor , _MidColor , float4( (float3( 0,0,0 ) + (( PreRamp210 * ( temp_output_219_0 - temp_cast_12 ) ) - float3( 0,0,0 )) * (float3( 1,1,1 ) - float3( 0,0,0 )) / (temp_cast_13 - float3( 0,0,0 ))) , 0.0 ));
				float3 temp_cast_15 = (temp_output_215_0).xxx;
				float4 lerpResult225 = lerp( _MidColor , _WhiteColor , float4( temp_output_219_0 , 0.0 ));
				float4 lerpResult226 = lerp( lerpResult220 , lerpResult225 , float4( PreRamp210 , 0.0 ));
				float4 break230 = lerpResult226;
				float4 appendResult231 = (float4(break230.r , break230.g , break230.b , PreRamp210.x));
				float4 PostRamp232 = appendResult231;
				#ifdef _USERAMP_ON
				float4 staticSwitch236 = PostRamp232;
				#else
				float4 staticSwitch236 = float4( ( ( desaturateVar166 * MultiplyNoise92 ) + AdditiveNoise91 ) , 0.0 );
				#endif
				float4 texCoord71 = i.ase_texcoord1;
				texCoord71.xy = i.ase_texcoord1.xy * float2( 1,1 ) + float2( 0,0 );
				float4 temp_output_39_0 = ( i.ase_color * staticSwitch236 * ( texCoord71.z + 1.0 ) );
				float4 texCoord258 = i.ase_texcoord1;
				texCoord258.xy = i.ase_texcoord1.xy * float2( 1,1 ) + float2( 0,0 );
				float2 _Vector0 = float2(-0.25,1);
				float temp_output_414_0 = (_Vector0.x + (( texCoord258.w + -1.0 ) - 0.0) * (_Vector0.y - _Vector0.x) / (1.0 - 0.0));
				float4 break17_g211 = tex2DNode79;
				float4 appendResult18_g211 = (float4(break17_g211.x , break17_g211.y , break17_g211.z , break17_g211.w));
				float4 clampResult19_g211 = clamp( ( appendResult18_g211 * _DetailDisolveChannel ) , float4( 0,0,0,0 ) , float4( 1,1,1,1 ) );
				float4 break2_g211 = clampResult19_g211;
				float clampResult20_g211 = clamp( ( break2_g211.x + break2_g211.y + break2_g211.z + break2_g211.w ) , 0.0 , 1.0 );
				float DisolveNoise275 = clampResult20_g211;
				float smoothstepResult416 = smoothstep( temp_output_414_0 , ( temp_output_414_0 + 0.25 ) , DisolveNoise275);
				#ifdef _DISABLEEROSION_ON
				float staticSwitch417 = 1.0;
				#else
				float staticSwitch417 = saturate( smoothstepResult416 );
				#endif
				float2 uv_AlphaOverride = i.ase_texcoord1.xy * _AlphaOverride_ST.xy + _AlphaOverride_ST.zw;
				float2 panner44 = ( 1.0 * _Time.y * _AlphaOverridePanning + uv_AlphaOverride);
				float4 break2_g205 = ( tex2D( _AlphaOverride, panner44 ) * _AlphaOverrideChannel );
				float AlphaOverride49 = saturate( ( break2_g205.x + break2_g205.y + break2_g205.z + break2_g205.w ) );
				#ifdef _USEALPHAOVERRIDE_ON
				float staticSwitch313 = AlphaOverride49;
				#else
				float staticSwitch313 = 1.0;
				#endif
				float2 panner33 = ( 1.0 * _Time.y * _MainTexturePanning + ( UVModifiers204 + staticSwitch456 ));
				float4 break2_g210 = ( tex2D( _MainTex, panner33 ) * _MainAlphaChannel );
				float MainAlpha30 = saturate( ( break2_g210.x + break2_g210.y + break2_g210.z + break2_g210.w ) );
				float temp_output_55_0 = ( staticSwitch313 * MainAlpha30 );
				float4 screenPos = i.ase_texcoord3;
				float4 ase_screenPosNorm = screenPos / screenPos.w;
				ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
				float screenDepth199 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy ));
				float distanceDepth199 = abs( ( screenDepth199 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( _SoftFadeFactor ) );
				#ifdef _USESOFTALPHA_ON
				float staticSwitch198 = ( temp_output_55_0 * saturate( distanceDepth199 ) );
				#else
				float staticSwitch198 = temp_output_55_0;
				#endif
				float temp_output_396_0 = ( ( staticSwitch417 * staticSwitch198 ) * i.ase_color.a );
				float3 ase_worldPos = i.ase_texcoord4.xyz;
				float3 ase_worldViewDir = UnityWorldSpaceViewDir(ase_worldPos);
				ase_worldViewDir = normalize(ase_worldViewDir);
				float3 ase_worldNormal = i.ase_texcoord5.xyz;
				float fresnelNdotV406 = dot( ase_worldNormal, ase_worldViewDir );
				float fresnelNode406 = ( 0.0 + _FresnelScale * pow( max( 1.0 - fresnelNdotV406 , 0.0001 ), _FresnelPower ) );
				float4 lerpResult410 = lerp( temp_output_39_0 , _FresnelColor , fresnelNode406);
				#ifdef _FRESNEL_ON
				float4 staticSwitch403 = ( temp_output_396_0 * lerpResult410 );
				#else
				float4 staticSwitch403 = temp_output_39_0;
				#endif
				float4 break459 = staticSwitch403;
				float3 appendResult460 = (float3(break459.r , break459.g , break459.b));
				
				
				outColor = appendResult460;
				outAlpha = temp_output_396_0;
				clip(outAlpha);
				return float4(outColor,outAlpha);
			}
			ENDCG
		}
		
		
		Pass
		{
			Name "ForwardAdd"
			Tags { "LightMode"="ForwardAdd" }
			ZWrite Off
			Blend One One
			CGPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fwdadd_fullshadows
			#ifndef UNITY_PASS_FORWARDADD
			#define UNITY_PASS_FORWARDADD
			#endif
			#include "UnityCG.cginc"
			#include "UnityShaderVariables.cginc"
			#define ASE_NEEDS_VERT_NORMAL
			#define ASE_NEEDS_FRAG_COLOR
			#pragma shader_feature_local _FRESNEL_ON
			#pragma shader_feature_local _USERAMP_ON
			#pragma shader_feature_local _USEPIXELATION_ON
			#pragma shader_feature_local _USEUVOFFSET_ON
			#pragma shader_feature_local _DISABLEEROSION_ON
			#pragma shader_feature_local _USESOFTALPHA_ON
			#pragma shader_feature_local _USEALPHAOVERRIDE_ON

			uniform float _SourceBlendRGB;
			uniform float _Culling;
			uniform float _AlphaClip;
			uniform sampler2D _DetailNoise;
			uniform float2 _DetailNoisePanning;
			uniform float4 _DetailNoise_ST;
			uniform float4 _DetailVertexOffsetChannel;
			uniform sampler2D _MainTex;
			uniform float2 _MainTexturePanning;
			uniform float4 _MainTex_ST;
			uniform float2 _Resolution;
			uniform float4 _DetailDistortionChannel;
			uniform float _DistortionIntensity;
			uniform float4 _MainTextureChannel;
			uniform float _Desaturate;
			uniform float4 _DetailMultiplyChannel;
			uniform float _MultiplyNoiseDesaturation;
			uniform float4 _DetailAdditiveChannel;
			uniform float4 _LastColor;
			uniform float4 _MidColor;
			uniform float _MiddlePointPos;
			uniform float _MiddlePointPos1;
			uniform float4 _WhiteColor;
			uniform float4 _DetailDisolveChannel;
			uniform sampler2D _AlphaOverride;
			uniform float2 _AlphaOverridePanning;
			uniform float4 _AlphaOverride_ST;
			uniform float4 _AlphaOverrideChannel;
			uniform float4 _MainAlphaChannel;
			UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
			uniform float4 _CameraDepthTexture_TexelSize;
			uniform float _SoftFadeFactor;
			uniform float4 _FresnelColor;
			uniform float _FresnelScale;
			uniform float _FresnelPower;


			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				float4 ase_texcoord1 : TEXCOORD1;
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_color : COLOR;
			};
			
			struct v2f
			{
				float4 pos : SV_POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
				float4 ase_color : COLOR;
				float4 ase_texcoord1 : TEXCOORD1;
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_texcoord3 : TEXCOORD3;
				float4 ase_texcoord4 : TEXCOORD4;
				float4 ase_texcoord5 : TEXCOORD5;
			};
			
			v2f vert ( appdata v )
			{
				v2f o;
				UNITY_INITIALIZE_OUTPUT(v2f,o);
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				
				float4 texCoord429 = v.ase_texcoord1;
				texCoord429.xy = v.ase_texcoord1.xy * float2( 1,1 ) + float2( 0,0 );
				float2 uv_DetailNoise = v.ase_texcoord * _DetailNoise_ST.xy + _DetailNoise_ST.zw;
				float2 panner80 = ( 1.0 * _Time.y * _DetailNoisePanning + uv_DetailNoise);
				float4 tex2DNode79 = tex2Dlod( _DetailNoise, float4( panner80, 0, 0.0) );
				float4 break17_g214 = tex2DNode79;
				float4 appendResult18_g214 = (float4(break17_g214.x , break17_g214.y , break17_g214.z , break17_g214.w));
				float4 clampResult19_g214 = clamp( ( appendResult18_g214 * _DetailVertexOffsetChannel ) , float4( 0,0,0,0 ) , float4( 1,1,1,1 ) );
				float4 break2_g214 = clampResult19_g214;
				float clampResult20_g214 = clamp( ( break2_g214.x + break2_g214.y + break2_g214.z + break2_g214.w ) , 0.0 , 1.0 );
				float VertexOffset434 = clampResult20_g214;
				
				float4 ase_clipPos = UnityObjectToClipPos(v.vertex);
				float4 screenPos = ComputeScreenPos(ase_clipPos);
				o.ase_texcoord3 = screenPos;
				float3 ase_worldPos = mul(unity_ObjectToWorld, float4( (v.vertex).xyz, 1 )).xyz;
				o.ase_texcoord4.xyz = ase_worldPos;
				float3 ase_worldNormal = UnityObjectToWorldNormal(v.normal);
				o.ase_texcoord5.xyz = ase_worldNormal;
				
				o.ase_color = v.ase_color;
				o.ase_texcoord1 = v.ase_texcoord;
				o.ase_texcoord2 = v.ase_texcoord1;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord4.w = 0;
				o.ase_texcoord5.w = 0;
				
				v.vertex.xyz += ( ( texCoord429.z * VertexOffset434 ) * v.normal );
				o.pos = UnityObjectToClipPos(v.vertex);
				#if ASE_SHADOWS
					#if UNITY_VERSION >= 560
						UNITY_TRANSFER_SHADOW( o, v.texcoord );
					#else
						TRANSFER_SHADOW( o );
					#endif
				#endif
				return o;
			}
			
			float4 frag (v2f i ) : SV_Target
			{
				float3 outColor;
				float outAlpha;

				float2 uv_MainTex = i.ase_texcoord1.xy * _MainTex_ST.xy + _MainTex_ST.zw;
				float2 break446 = uv_MainTex;
				float2 appendResult455 = (float2(( break446.x - ( fmod( break446.x , ( 1.0 / _Resolution ).x ) - ( ( 1.0 / _Resolution ).x * 0.5 ) ) ) , ( break446.y - ( fmod( break446.y , 0.0 ) - ( 0.0 * 0.5 ) ) )));
				#ifdef _USEPIXELATION_ON
				float2 staticSwitch456 = appendResult455;
				#else
				float2 staticSwitch456 = uv_MainTex;
				#endif
				float2 uv_DetailNoise = i.ase_texcoord1.xy * _DetailNoise_ST.xy + _DetailNoise_ST.zw;
				float2 panner80 = ( 1.0 * _Time.y * _DetailNoisePanning + uv_DetailNoise);
				float4 tex2DNode79 = tex2D( _DetailNoise, panner80 );
				float4 break17_g202 = tex2DNode79;
				float4 appendResult18_g202 = (float4(break17_g202.x , break17_g202.y , break17_g202.z , break17_g202.w));
				float4 clampResult19_g202 = clamp( ( appendResult18_g202 * _DetailDistortionChannel ) , float4( 0,0,0,0 ) , float4( 1,1,1,1 ) );
				float4 break2_g202 = clampResult19_g202;
				float clampResult20_g202 = clamp( ( break2_g202.x + break2_g202.y + break2_g202.z + break2_g202.w ) , 0.0 , 1.0 );
				float DistortionNoise90 = clampResult20_g202;
				float temp_output_284_0 = ( DistortionNoise90 * _DistortionIntensity );
				float2 temp_cast_1 = (temp_output_284_0).xx;
				float4 texCoord397 = i.ase_texcoord2;
				texCoord397.xy = i.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float2 appendResult400 = (float2(texCoord397.x , texCoord397.y));
				#ifdef _USEUVOFFSET_ON
				float2 staticSwitch402 = ( temp_output_284_0 + appendResult400 );
				#else
				float2 staticSwitch402 = temp_cast_1;
				#endif
				float2 UVModifiers204 = staticSwitch402;
				float2 panner22 = ( 1.0 * _Time.y * _MainTexturePanning + ( staticSwitch456 + UVModifiers204 ));
				float4 tex2DNode6 = tex2D( _MainTex, panner22 );
				float4 break376 = tex2DNode6;
				float4 break379 = _MainTextureChannel;
				float4 appendResult375 = (float4(( break376.r * break379.x ) , ( break376.g * break379.y ) , ( break376.b * break379.z ) , ( break376.a * break379.w )));
				float4 MainTexInfo25 = appendResult375;
				float3 desaturateInitialColor166 = MainTexInfo25.xyz;
				float desaturateDot166 = dot( desaturateInitialColor166, float3( 0.299, 0.587, 0.114 ));
				float3 desaturateVar166 = lerp( desaturateInitialColor166, desaturateDot166.xxx, _Desaturate );
				float4 break364 = ( _DetailMultiplyChannel * tex2DNode79 );
				float4 appendResult365 = (float4(break364.x , break364.y , break364.z , break364.w));
				float3 desaturateInitialColor362 = appendResult365.xyz;
				float desaturateDot362 = dot( desaturateInitialColor362, float3( 0.299, 0.587, 0.114 ));
				float3 desaturateVar362 = lerp( desaturateInitialColor362, desaturateDot362.xxx, _MultiplyNoiseDesaturation );
				float3 temp_cast_5 = (1.0).xxx;
				float3 temp_cast_6 = (1.0).xxx;
				float3 ifLocalVar106 = 0;
				if( ( _DetailMultiplyChannel.x + _DetailMultiplyChannel.y + _DetailMultiplyChannel.z + _DetailMultiplyChannel.w ) <= 0.0 )
				ifLocalVar106 = temp_cast_6;
				else
				ifLocalVar106 = desaturateVar362;
				float3 MultiplyNoise92 = ifLocalVar106;
				float4 break156 = ( _DetailAdditiveChannel * tex2DNode79 );
				float4 appendResult155 = (float4(break156.x , break156.y , break156.z , break156.w));
				float3 desaturateInitialColor191 = appendResult155.xyz;
				float desaturateDot191 = dot( desaturateInitialColor191, float3( 0.299, 0.587, 0.114 ));
				float3 desaturateVar191 = lerp( desaturateInitialColor191, desaturateDot191.xxx, 1.0 );
				float3 AdditiveNoise91 = desaturateVar191;
				float3 PreRamp210 = desaturateVar166;
				float3 temp_cast_10 = (_MiddlePointPos).xxx;
				float3 clampResult218 = clamp( ( PreRamp210 - temp_cast_10 ) , float3( 0,0,0 ) , float3( 1,1,1 ) );
				float temp_output_215_0 = ( 1.0 - _MiddlePointPos );
				float3 temp_cast_11 = (temp_output_215_0).xxx;
				float3 temp_output_219_0 = (float3( 0,0,0 ) + (clampResult218 - float3( 0,0,0 )) * (float3( 1,1,1 ) - float3( 0,0,0 )) / (temp_cast_11 - float3( 0,0,0 )));
				float3 temp_cast_12 = (_MiddlePointPos1).xxx;
				float3 temp_cast_13 = (temp_output_215_0).xxx;
				float4 lerpResult220 = lerp( _LastColor , _MidColor , float4( (float3( 0,0,0 ) + (( PreRamp210 * ( temp_output_219_0 - temp_cast_12 ) ) - float3( 0,0,0 )) * (float3( 1,1,1 ) - float3( 0,0,0 )) / (temp_cast_13 - float3( 0,0,0 ))) , 0.0 ));
				float3 temp_cast_15 = (temp_output_215_0).xxx;
				float4 lerpResult225 = lerp( _MidColor , _WhiteColor , float4( temp_output_219_0 , 0.0 ));
				float4 lerpResult226 = lerp( lerpResult220 , lerpResult225 , float4( PreRamp210 , 0.0 ));
				float4 break230 = lerpResult226;
				float4 appendResult231 = (float4(break230.r , break230.g , break230.b , PreRamp210.x));
				float4 PostRamp232 = appendResult231;
				#ifdef _USERAMP_ON
				float4 staticSwitch236 = PostRamp232;
				#else
				float4 staticSwitch236 = float4( ( ( desaturateVar166 * MultiplyNoise92 ) + AdditiveNoise91 ) , 0.0 );
				#endif
				float4 texCoord71 = i.ase_texcoord1;
				texCoord71.xy = i.ase_texcoord1.xy * float2( 1,1 ) + float2( 0,0 );
				float4 temp_output_39_0 = ( i.ase_color * staticSwitch236 * ( texCoord71.z + 1.0 ) );
				float4 texCoord258 = i.ase_texcoord1;
				texCoord258.xy = i.ase_texcoord1.xy * float2( 1,1 ) + float2( 0,0 );
				float2 _Vector0 = float2(-0.25,1);
				float temp_output_414_0 = (_Vector0.x + (( texCoord258.w + -1.0 ) - 0.0) * (_Vector0.y - _Vector0.x) / (1.0 - 0.0));
				float4 break17_g211 = tex2DNode79;
				float4 appendResult18_g211 = (float4(break17_g211.x , break17_g211.y , break17_g211.z , break17_g211.w));
				float4 clampResult19_g211 = clamp( ( appendResult18_g211 * _DetailDisolveChannel ) , float4( 0,0,0,0 ) , float4( 1,1,1,1 ) );
				float4 break2_g211 = clampResult19_g211;
				float clampResult20_g211 = clamp( ( break2_g211.x + break2_g211.y + break2_g211.z + break2_g211.w ) , 0.0 , 1.0 );
				float DisolveNoise275 = clampResult20_g211;
				float smoothstepResult416 = smoothstep( temp_output_414_0 , ( temp_output_414_0 + 0.25 ) , DisolveNoise275);
				#ifdef _DISABLEEROSION_ON
				float staticSwitch417 = 1.0;
				#else
				float staticSwitch417 = saturate( smoothstepResult416 );
				#endif
				float2 uv_AlphaOverride = i.ase_texcoord1.xy * _AlphaOverride_ST.xy + _AlphaOverride_ST.zw;
				float2 panner44 = ( 1.0 * _Time.y * _AlphaOverridePanning + uv_AlphaOverride);
				float4 break2_g205 = ( tex2D( _AlphaOverride, panner44 ) * _AlphaOverrideChannel );
				float AlphaOverride49 = saturate( ( break2_g205.x + break2_g205.y + break2_g205.z + break2_g205.w ) );
				#ifdef _USEALPHAOVERRIDE_ON
				float staticSwitch313 = AlphaOverride49;
				#else
				float staticSwitch313 = 1.0;
				#endif
				float2 panner33 = ( 1.0 * _Time.y * _MainTexturePanning + ( UVModifiers204 + staticSwitch456 ));
				float4 break2_g210 = ( tex2D( _MainTex, panner33 ) * _MainAlphaChannel );
				float MainAlpha30 = saturate( ( break2_g210.x + break2_g210.y + break2_g210.z + break2_g210.w ) );
				float temp_output_55_0 = ( staticSwitch313 * MainAlpha30 );
				float4 screenPos = i.ase_texcoord3;
				float4 ase_screenPosNorm = screenPos / screenPos.w;
				ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
				float screenDepth199 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy ));
				float distanceDepth199 = abs( ( screenDepth199 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( _SoftFadeFactor ) );
				#ifdef _USESOFTALPHA_ON
				float staticSwitch198 = ( temp_output_55_0 * saturate( distanceDepth199 ) );
				#else
				float staticSwitch198 = temp_output_55_0;
				#endif
				float temp_output_396_0 = ( ( staticSwitch417 * staticSwitch198 ) * i.ase_color.a );
				float3 ase_worldPos = i.ase_texcoord4.xyz;
				float3 ase_worldViewDir = UnityWorldSpaceViewDir(ase_worldPos);
				ase_worldViewDir = normalize(ase_worldViewDir);
				float3 ase_worldNormal = i.ase_texcoord5.xyz;
				float fresnelNdotV406 = dot( ase_worldNormal, ase_worldViewDir );
				float fresnelNode406 = ( 0.0 + _FresnelScale * pow( max( 1.0 - fresnelNdotV406 , 0.0001 ), _FresnelPower ) );
				float4 lerpResult410 = lerp( temp_output_39_0 , _FresnelColor , fresnelNode406);
				#ifdef _FRESNEL_ON
				float4 staticSwitch403 = ( temp_output_396_0 * lerpResult410 );
				#else
				float4 staticSwitch403 = temp_output_39_0;
				#endif
				float4 break459 = staticSwitch403;
				float3 appendResult460 = (float3(break459.r , break459.g , break459.b));
				
				
				outColor = appendResult460;
				outAlpha = temp_output_396_0;
				clip(outAlpha);
				return float4(outColor,outAlpha);
			}
			ENDCG
		}

		
		Pass
		{
			Name "Deferred"
			Tags { "LightMode"="Deferred" }

			CGINCLUDE
			#pragma target 3.0
			ENDCG
			Blend Off
			AlphaToMask Off
			Cull Back
			ColorMask RGBA
			ZWrite On
			ZTest LEqual
			Offset 0 , 0
			
			CGPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_prepassfinal
			#ifndef UNITY_PASS_DEFERRED
			#define UNITY_PASS_DEFERRED
			#endif
			#include "UnityCG.cginc"
			#include "UnityShaderVariables.cginc"
			#define ASE_NEEDS_VERT_NORMAL

			uniform float _SourceBlendRGB;
			uniform float _Culling;
			uniform float _AlphaClip;
			uniform sampler2D _DetailNoise;
			uniform float2 _DetailNoisePanning;
			uniform float4 _DetailNoise_ST;
			uniform float4 _DetailVertexOffsetChannel;


			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				float4 ase_texcoord1 : TEXCOORD1;
				float4 ase_texcoord : TEXCOORD0;
			};
			
			struct v2f
			{
				float4 pos : SV_POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
				
			};
			
			v2f vert ( appdata v )
			{
				v2f o;
				UNITY_INITIALIZE_OUTPUT(v2f,o);
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				
				float4 texCoord429 = v.ase_texcoord1;
				texCoord429.xy = v.ase_texcoord1.xy * float2( 1,1 ) + float2( 0,0 );
				float2 uv_DetailNoise = v.ase_texcoord.xy * _DetailNoise_ST.xy + _DetailNoise_ST.zw;
				float2 panner80 = ( 1.0 * _Time.y * _DetailNoisePanning + uv_DetailNoise);
				float4 tex2DNode79 = tex2Dlod( _DetailNoise, float4( panner80, 0, 0.0) );
				float4 break17_g214 = tex2DNode79;
				float4 appendResult18_g214 = (float4(break17_g214.x , break17_g214.y , break17_g214.z , break17_g214.w));
				float4 clampResult19_g214 = clamp( ( appendResult18_g214 * _DetailVertexOffsetChannel ) , float4( 0,0,0,0 ) , float4( 1,1,1,1 ) );
				float4 break2_g214 = clampResult19_g214;
				float clampResult20_g214 = clamp( ( break2_g214.x + break2_g214.y + break2_g214.z + break2_g214.w ) , 0.0 , 1.0 );
				float VertexOffset434 = clampResult20_g214;
				
				
				v.vertex.xyz += ( ( texCoord429.z * VertexOffset434 ) * v.normal );
				o.pos = UnityObjectToClipPos(v.vertex);
				#if ASE_SHADOWS
					#if UNITY_VERSION >= 560
						UNITY_TRANSFER_SHADOW( o, v.texcoord );
					#else
						TRANSFER_SHADOW( o );
					#endif
				#endif
				return o;
			}
			
			void frag (v2f i , out half4 outGBuffer0 : SV_Target0, out half4 outGBuffer1 : SV_Target1, out half4 outGBuffer2 : SV_Target2, out half4 outGBuffer3 : SV_Target3)
			{
				
				
				outGBuffer0 = 0;
				outGBuffer1 = 0;
				outGBuffer2 = 0;
				outGBuffer3 = 0;
			}
			ENDCG
		}
		
		
		Pass
		{
			
			Name "ShadowCaster"
			Tags { "LightMode"="ShadowCaster" }
			ZWrite On
			ZTest LEqual
			CGPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_shadowcaster
			#ifndef UNITY_PASS_SHADOWCASTER
			#define UNITY_PASS_SHADOWCASTER
			#endif
			#include "UnityCG.cginc"
			#include "UnityShaderVariables.cginc"
			#define ASE_NEEDS_VERT_NORMAL
			#define ASE_NEEDS_FRAG_COLOR
			#pragma shader_feature_local _FRESNEL_ON
			#pragma shader_feature_local _USERAMP_ON
			#pragma shader_feature_local _USEPIXELATION_ON
			#pragma shader_feature_local _USEUVOFFSET_ON
			#pragma shader_feature_local _DISABLEEROSION_ON
			#pragma shader_feature_local _USESOFTALPHA_ON
			#pragma shader_feature_local _USEALPHAOVERRIDE_ON

			uniform float _SourceBlendRGB;
			uniform float _Culling;
			uniform float _AlphaClip;
			uniform sampler2D _DetailNoise;
			uniform float2 _DetailNoisePanning;
			uniform float4 _DetailNoise_ST;
			uniform float4 _DetailVertexOffsetChannel;
			uniform sampler2D _MainTex;
			uniform float2 _MainTexturePanning;
			uniform float4 _MainTex_ST;
			uniform float2 _Resolution;
			uniform float4 _DetailDistortionChannel;
			uniform float _DistortionIntensity;
			uniform float4 _MainTextureChannel;
			uniform float _Desaturate;
			uniform float4 _DetailMultiplyChannel;
			uniform float _MultiplyNoiseDesaturation;
			uniform float4 _DetailAdditiveChannel;
			uniform float4 _LastColor;
			uniform float4 _MidColor;
			uniform float _MiddlePointPos;
			uniform float _MiddlePointPos1;
			uniform float4 _WhiteColor;
			uniform float4 _DetailDisolveChannel;
			uniform sampler2D _AlphaOverride;
			uniform float2 _AlphaOverridePanning;
			uniform float4 _AlphaOverride_ST;
			uniform float4 _AlphaOverrideChannel;
			uniform float4 _MainAlphaChannel;
			UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
			uniform float4 _CameraDepthTexture_TexelSize;
			uniform float _SoftFadeFactor;
			uniform float4 _FresnelColor;
			uniform float _FresnelScale;
			uniform float _FresnelPower;


			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				float4 ase_texcoord1 : TEXCOORD1;
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_color : COLOR;
			};
			
			struct v2f
			{
				V2F_SHADOW_CASTER;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
				float4 ase_color : COLOR;
				float4 ase_texcoord1 : TEXCOORD1;
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_texcoord3 : TEXCOORD3;
				float4 ase_texcoord4 : TEXCOORD4;
				float4 ase_texcoord5 : TEXCOORD5;
			};

			
			v2f vert ( appdata v )
			{
				v2f o;
				UNITY_INITIALIZE_OUTPUT(v2f,o);
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				
				float4 texCoord429 = v.ase_texcoord1;
				texCoord429.xy = v.ase_texcoord1.xy * float2( 1,1 ) + float2( 0,0 );
				float2 uv_DetailNoise = v.ase_texcoord * _DetailNoise_ST.xy + _DetailNoise_ST.zw;
				float2 panner80 = ( 1.0 * _Time.y * _DetailNoisePanning + uv_DetailNoise);
				float4 tex2DNode79 = tex2Dlod( _DetailNoise, float4( panner80, 0, 0.0) );
				float4 break17_g214 = tex2DNode79;
				float4 appendResult18_g214 = (float4(break17_g214.x , break17_g214.y , break17_g214.z , break17_g214.w));
				float4 clampResult19_g214 = clamp( ( appendResult18_g214 * _DetailVertexOffsetChannel ) , float4( 0,0,0,0 ) , float4( 1,1,1,1 ) );
				float4 break2_g214 = clampResult19_g214;
				float clampResult20_g214 = clamp( ( break2_g214.x + break2_g214.y + break2_g214.z + break2_g214.w ) , 0.0 , 1.0 );
				float VertexOffset434 = clampResult20_g214;
				
				float4 ase_clipPos = UnityObjectToClipPos(v.vertex);
				float4 screenPos = ComputeScreenPos(ase_clipPos);
				o.ase_texcoord3 = screenPos;
				float3 ase_worldPos = mul(unity_ObjectToWorld, float4( (v.vertex).xyz, 1 )).xyz;
				o.ase_texcoord4.xyz = ase_worldPos;
				float3 ase_worldNormal = UnityObjectToWorldNormal(v.normal);
				o.ase_texcoord5.xyz = ase_worldNormal;
				
				o.ase_color = v.ase_color;
				o.ase_texcoord1 = v.ase_texcoord;
				o.ase_texcoord2 = v.ase_texcoord1;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord4.w = 0;
				o.ase_texcoord5.w = 0;
				
				v.vertex.xyz += ( ( texCoord429.z * VertexOffset434 ) * v.normal );
				TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
				return o;
			}
			
			float4 frag (v2f i ) : SV_Target
			{
				float3 outColor;
				float outAlpha;

				float2 uv_MainTex = i.ase_texcoord1.xy * _MainTex_ST.xy + _MainTex_ST.zw;
				float2 break446 = uv_MainTex;
				float2 appendResult455 = (float2(( break446.x - ( fmod( break446.x , ( 1.0 / _Resolution ).x ) - ( ( 1.0 / _Resolution ).x * 0.5 ) ) ) , ( break446.y - ( fmod( break446.y , 0.0 ) - ( 0.0 * 0.5 ) ) )));
				#ifdef _USEPIXELATION_ON
				float2 staticSwitch456 = appendResult455;
				#else
				float2 staticSwitch456 = uv_MainTex;
				#endif
				float2 uv_DetailNoise = i.ase_texcoord1.xy * _DetailNoise_ST.xy + _DetailNoise_ST.zw;
				float2 panner80 = ( 1.0 * _Time.y * _DetailNoisePanning + uv_DetailNoise);
				float4 tex2DNode79 = tex2D( _DetailNoise, panner80 );
				float4 break17_g202 = tex2DNode79;
				float4 appendResult18_g202 = (float4(break17_g202.x , break17_g202.y , break17_g202.z , break17_g202.w));
				float4 clampResult19_g202 = clamp( ( appendResult18_g202 * _DetailDistortionChannel ) , float4( 0,0,0,0 ) , float4( 1,1,1,1 ) );
				float4 break2_g202 = clampResult19_g202;
				float clampResult20_g202 = clamp( ( break2_g202.x + break2_g202.y + break2_g202.z + break2_g202.w ) , 0.0 , 1.0 );
				float DistortionNoise90 = clampResult20_g202;
				float temp_output_284_0 = ( DistortionNoise90 * _DistortionIntensity );
				float2 temp_cast_1 = (temp_output_284_0).xx;
				float4 texCoord397 = i.ase_texcoord2;
				texCoord397.xy = i.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float2 appendResult400 = (float2(texCoord397.x , texCoord397.y));
				#ifdef _USEUVOFFSET_ON
				float2 staticSwitch402 = ( temp_output_284_0 + appendResult400 );
				#else
				float2 staticSwitch402 = temp_cast_1;
				#endif
				float2 UVModifiers204 = staticSwitch402;
				float2 panner22 = ( 1.0 * _Time.y * _MainTexturePanning + ( staticSwitch456 + UVModifiers204 ));
				float4 tex2DNode6 = tex2D( _MainTex, panner22 );
				float4 break376 = tex2DNode6;
				float4 break379 = _MainTextureChannel;
				float4 appendResult375 = (float4(( break376.r * break379.x ) , ( break376.g * break379.y ) , ( break376.b * break379.z ) , ( break376.a * break379.w )));
				float4 MainTexInfo25 = appendResult375;
				float3 desaturateInitialColor166 = MainTexInfo25.xyz;
				float desaturateDot166 = dot( desaturateInitialColor166, float3( 0.299, 0.587, 0.114 ));
				float3 desaturateVar166 = lerp( desaturateInitialColor166, desaturateDot166.xxx, _Desaturate );
				float4 break364 = ( _DetailMultiplyChannel * tex2DNode79 );
				float4 appendResult365 = (float4(break364.x , break364.y , break364.z , break364.w));
				float3 desaturateInitialColor362 = appendResult365.xyz;
				float desaturateDot362 = dot( desaturateInitialColor362, float3( 0.299, 0.587, 0.114 ));
				float3 desaturateVar362 = lerp( desaturateInitialColor362, desaturateDot362.xxx, _MultiplyNoiseDesaturation );
				float3 temp_cast_5 = (1.0).xxx;
				float3 temp_cast_6 = (1.0).xxx;
				float3 ifLocalVar106 = 0;
				if( ( _DetailMultiplyChannel.x + _DetailMultiplyChannel.y + _DetailMultiplyChannel.z + _DetailMultiplyChannel.w ) <= 0.0 )
				ifLocalVar106 = temp_cast_6;
				else
				ifLocalVar106 = desaturateVar362;
				float3 MultiplyNoise92 = ifLocalVar106;
				float4 break156 = ( _DetailAdditiveChannel * tex2DNode79 );
				float4 appendResult155 = (float4(break156.x , break156.y , break156.z , break156.w));
				float3 desaturateInitialColor191 = appendResult155.xyz;
				float desaturateDot191 = dot( desaturateInitialColor191, float3( 0.299, 0.587, 0.114 ));
				float3 desaturateVar191 = lerp( desaturateInitialColor191, desaturateDot191.xxx, 1.0 );
				float3 AdditiveNoise91 = desaturateVar191;
				float3 PreRamp210 = desaturateVar166;
				float3 temp_cast_10 = (_MiddlePointPos).xxx;
				float3 clampResult218 = clamp( ( PreRamp210 - temp_cast_10 ) , float3( 0,0,0 ) , float3( 1,1,1 ) );
				float temp_output_215_0 = ( 1.0 - _MiddlePointPos );
				float3 temp_cast_11 = (temp_output_215_0).xxx;
				float3 temp_output_219_0 = (float3( 0,0,0 ) + (clampResult218 - float3( 0,0,0 )) * (float3( 1,1,1 ) - float3( 0,0,0 )) / (temp_cast_11 - float3( 0,0,0 )));
				float3 temp_cast_12 = (_MiddlePointPos1).xxx;
				float3 temp_cast_13 = (temp_output_215_0).xxx;
				float4 lerpResult220 = lerp( _LastColor , _MidColor , float4( (float3( 0,0,0 ) + (( PreRamp210 * ( temp_output_219_0 - temp_cast_12 ) ) - float3( 0,0,0 )) * (float3( 1,1,1 ) - float3( 0,0,0 )) / (temp_cast_13 - float3( 0,0,0 ))) , 0.0 ));
				float3 temp_cast_15 = (temp_output_215_0).xxx;
				float4 lerpResult225 = lerp( _MidColor , _WhiteColor , float4( temp_output_219_0 , 0.0 ));
				float4 lerpResult226 = lerp( lerpResult220 , lerpResult225 , float4( PreRamp210 , 0.0 ));
				float4 break230 = lerpResult226;
				float4 appendResult231 = (float4(break230.r , break230.g , break230.b , PreRamp210.x));
				float4 PostRamp232 = appendResult231;
				#ifdef _USERAMP_ON
				float4 staticSwitch236 = PostRamp232;
				#else
				float4 staticSwitch236 = float4( ( ( desaturateVar166 * MultiplyNoise92 ) + AdditiveNoise91 ) , 0.0 );
				#endif
				float4 texCoord71 = i.ase_texcoord1;
				texCoord71.xy = i.ase_texcoord1.xy * float2( 1,1 ) + float2( 0,0 );
				float4 temp_output_39_0 = ( i.ase_color * staticSwitch236 * ( texCoord71.z + 1.0 ) );
				float4 texCoord258 = i.ase_texcoord1;
				texCoord258.xy = i.ase_texcoord1.xy * float2( 1,1 ) + float2( 0,0 );
				float2 _Vector0 = float2(-0.25,1);
				float temp_output_414_0 = (_Vector0.x + (( texCoord258.w + -1.0 ) - 0.0) * (_Vector0.y - _Vector0.x) / (1.0 - 0.0));
				float4 break17_g211 = tex2DNode79;
				float4 appendResult18_g211 = (float4(break17_g211.x , break17_g211.y , break17_g211.z , break17_g211.w));
				float4 clampResult19_g211 = clamp( ( appendResult18_g211 * _DetailDisolveChannel ) , float4( 0,0,0,0 ) , float4( 1,1,1,1 ) );
				float4 break2_g211 = clampResult19_g211;
				float clampResult20_g211 = clamp( ( break2_g211.x + break2_g211.y + break2_g211.z + break2_g211.w ) , 0.0 , 1.0 );
				float DisolveNoise275 = clampResult20_g211;
				float smoothstepResult416 = smoothstep( temp_output_414_0 , ( temp_output_414_0 + 0.25 ) , DisolveNoise275);
				#ifdef _DISABLEEROSION_ON
				float staticSwitch417 = 1.0;
				#else
				float staticSwitch417 = saturate( smoothstepResult416 );
				#endif
				float2 uv_AlphaOverride = i.ase_texcoord1.xy * _AlphaOverride_ST.xy + _AlphaOverride_ST.zw;
				float2 panner44 = ( 1.0 * _Time.y * _AlphaOverridePanning + uv_AlphaOverride);
				float4 break2_g205 = ( tex2D( _AlphaOverride, panner44 ) * _AlphaOverrideChannel );
				float AlphaOverride49 = saturate( ( break2_g205.x + break2_g205.y + break2_g205.z + break2_g205.w ) );
				#ifdef _USEALPHAOVERRIDE_ON
				float staticSwitch313 = AlphaOverride49;
				#else
				float staticSwitch313 = 1.0;
				#endif
				float2 panner33 = ( 1.0 * _Time.y * _MainTexturePanning + ( UVModifiers204 + staticSwitch456 ));
				float4 break2_g210 = ( tex2D( _MainTex, panner33 ) * _MainAlphaChannel );
				float MainAlpha30 = saturate( ( break2_g210.x + break2_g210.y + break2_g210.z + break2_g210.w ) );
				float temp_output_55_0 = ( staticSwitch313 * MainAlpha30 );
				float4 screenPos = i.ase_texcoord3;
				float4 ase_screenPosNorm = screenPos / screenPos.w;
				ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
				float screenDepth199 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy ));
				float distanceDepth199 = abs( ( screenDepth199 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( _SoftFadeFactor ) );
				#ifdef _USESOFTALPHA_ON
				float staticSwitch198 = ( temp_output_55_0 * saturate( distanceDepth199 ) );
				#else
				float staticSwitch198 = temp_output_55_0;
				#endif
				float temp_output_396_0 = ( ( staticSwitch417 * staticSwitch198 ) * i.ase_color.a );
				float3 ase_worldPos = i.ase_texcoord4.xyz;
				float3 ase_worldViewDir = UnityWorldSpaceViewDir(ase_worldPos);
				ase_worldViewDir = normalize(ase_worldViewDir);
				float3 ase_worldNormal = i.ase_texcoord5.xyz;
				float fresnelNdotV406 = dot( ase_worldNormal, ase_worldViewDir );
				float fresnelNode406 = ( 0.0 + _FresnelScale * pow( max( 1.0 - fresnelNdotV406 , 0.0001 ), _FresnelPower ) );
				float4 lerpResult410 = lerp( temp_output_39_0 , _FresnelColor , fresnelNode406);
				#ifdef _FRESNEL_ON
				float4 staticSwitch403 = ( temp_output_396_0 * lerpResult410 );
				#else
				float4 staticSwitch403 = temp_output_39_0;
				#endif
				float4 break459 = staticSwitch403;
				float3 appendResult460 = (float3(break459.r , break459.g , break459.b));
				
				
				outColor = appendResult460;
				outAlpha = temp_output_396_0;
				clip(outAlpha);
				SHADOW_CASTER_FRAGMENT(i)
			}
			ENDCG
		}
		
	}
	CustomEditor "ASEMaterialInspector"
	
	Fallback Off
}
/*ASEBEGIN
Version=19603
Node;AmplifyShaderEditor.CommentaryNode;103;-1311.825,-3308.272;Inherit;False;2247.985;1120.53;Extra Noise Setup;28;92;87;157;156;91;155;191;108;105;86;106;275;271;90;79;85;80;84;81;83;360;357;363;364;365;362;392;432;;0,0,0,1;0;0
Node;AmplifyShaderEditor.TexturePropertyNode;83;-1272.639,-3066.985;Inherit;True;Property;_DetailNoise;Detail Noise;15;0;Create;True;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.Vector2Node;84;-1018.617,-2776.467;Inherit;False;Property;_DetailNoisePanning;Detail Noise Panning;16;0;Create;True;0;0;0;False;0;False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.TextureCoordinatesNode;81;-1017.198,-2918.554;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PannerNode;80;-785.1999,-2817.554;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;79;-598.402,-3062.255;Inherit;True;Property;_TextureSample3;Texture Sample 3;4;0;Create;True;0;0;0;False;0;False;83;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.Vector4Node;85;-573.4172,-2854.065;Inherit;False;Property;_DetailDistortionChannel;Detail Distortion Channel;17;0;Create;True;0;0;0;False;0;False;0,0,0,0;0,0,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;34;-3765.718,-2210.248;Inherit;False;2101.975;1014.751;Main Texture Set Vars;26;383;382;25;10;6;375;376;379;394;393;380;381;30;164;12;28;299;205;288;22;300;33;287;23;206;27;;0,0,0,1;0;0
Node;AmplifyShaderEditor.FunctionNode;357;-286.5385,-2887.257;Inherit;False;Channel Picker;-1;;202;dc5f4cb24a8bdf448b40a1ec5866280e;0;2;5;FLOAT4;1,0,0,0;False;7;FLOAT4;0,0,0,1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;27;-3693.025,-1775.166;Inherit;True;Property;_MainTex;Main Texture;3;0;Create;False;0;0;0;False;0;False;None;367e6ca0002847af869880f8ed4d0163;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.RangedFloatNode;441;-5576.118,-2373.756;Inherit;False;Constant;_Float3;Float 1;4;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;442;-5597.232,-2250.141;Inherit;False;Property;_Resolution;Resolution;37;0;Create;True;0;0;0;False;0;False;64,64;64,64;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.CommentaryNode;197;-4985.138,-1466.259;Inherit;False;1151.407;785.7309;Set UV Modifiers For Main Tex;8;204;401;284;400;397;95;93;402;;0,0,0,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;90;-276.5659,-2798.716;Inherit;True;DistortionNoise;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;443;-5425.786,-2276.819;Inherit;False;2;0;FLOAT;1;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;286;-4451.507,-1825.872;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;93;-4895.138,-1408.26;Inherit;False;90;DistortionNoise;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;95;-4922.138,-1265.259;Inherit;False;Property;_DistortionIntensity;Distortion Intensity;18;0;Create;True;0;0;0;False;0;False;2;2;0;3;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;397;-4947.917,-1097.95;Inherit;False;1;-1;4;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.BreakToComponentsNode;444;-5279.232,-2258.141;Inherit;False;FLOAT2;1;0;FLOAT2;0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.RangedFloatNode;445;-5311.232,-2149.141;Inherit;False;Constant;_Float2;Float 0;4;0;Create;True;0;0;0;False;0;False;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.BreakToComponentsNode;446;-5135.468,-2722.654;Inherit;False;FLOAT2;1;0;FLOAT2;0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.DynamicAppendNode;400;-4687.568,-1086.344;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;284;-4611.727,-1328.067;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;447;-4901.232,-2451.14;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FmodOpNode;448;-4907.232,-2192.141;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;449;-4913.232,-2091.14;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FmodOpNode;450;-4888.232,-2588.141;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;401;-4503.568,-1186.344;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;451;-4734.232,-2537.141;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;452;-4746.232,-2134.14;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;402;-4354.689,-1330.466;Inherit;False;Property;_UseUVOffset;Use UV Offset;30;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT2;0,0;False;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT2;0,0;False;6;FLOAT2;0,0;False;7;FLOAT2;0,0;False;8;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;453;-4579.232,-2271.141;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;454;-4587.232,-2382.141;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;204;-4008.153,-1373.363;Inherit;True;UVModifiers;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;455;-4403.232,-2277.141;Inherit;True;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.StaticSwitch;456;-4144.075,-2006.563;Inherit;False;Property;_UsePixelation;Use Pixelation;36;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT2;0,0;False;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT2;0,0;False;6;FLOAT2;0,0;False;7;FLOAT2;0,0;False;8;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;205;-3617.06,-1577.097;Inherit;False;204;UVModifiers;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector2Node;23;-3416.466,-1861.533;Inherit;False;Property;_MainTexturePanning;Main Texture Panning ;6;0;Create;True;0;0;0;False;0;False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleAddOpNode;288;-3438.473,-1633.852;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PannerNode;22;-3013.887,-1649.839;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.WireNode;299;-3208.566,-1622.747;Inherit;False;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.SamplerNode;6;-2833.533,-1679.72;Inherit;True;Property;_TextureSample0;Texture Sample 0;0;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.Vector4Node;10;-2783.6,-1411.548;Inherit;False;Property;_MainTextureChannel;Main Texture Channel;4;0;Create;True;0;0;0;False;0;False;1,1,1,0;1,1,1,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.BreakToComponentsNode;379;-2557.34,-1404.684;Inherit;True;FLOAT4;1;0;FLOAT4;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.BreakToComponentsNode;376;-2525.558,-1573.728;Inherit;False;COLOR;1;0;COLOR;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;380;-2240.34,-1642.684;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;381;-2249.34,-1546.684;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;382;-2258.34,-1431.684;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;383;-2269.34,-1315.684;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;375;-2096.758,-1507.83;Inherit;True;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.CommentaryNode;209;-742.0294,-1775.639;Inherit;False;894.2443;492.3073;Apply Texture Value Modifiers;8;167;36;166;210;98;207;102;101;;0,0,0,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;25;-1897.531,-1665.72;Inherit;True;MainTexInfo;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;36;-637.8974,-1725.639;Inherit;False;25;MainTexInfo;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;167;-686.0294,-1556.916;Inherit;False;Property;_Desaturate;Desaturate? ;7;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.DesaturateOpNode;166;-644.9704,-1645.413;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CommentaryNode;233;-4039.848,-668.2441;Inherit;False;3080.293;1758.132;Color Ramping;22;424;425;215;214;225;220;224;231;395;218;232;212;217;219;222;223;230;226;213;426;427;428;;0,0,0,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;210;-407.0714,-1715.739;Inherit;False;PreRamp;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;426;-3935.074,-212.4839;Inherit;True;210;PreRamp;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;214;-3995.609,68.96428;Inherit;False;Property;_MiddlePointPos;Middle Point Position;26;0;Create;False;0;0;0;False;0;False;0.5;0.5;-1;0.99;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;395;-3661.879,-134.2051;Inherit;True;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CommentaryNode;50;-3748.647,-2778.444;Inherit;False;1894.862;545.101;Alpha Override;8;49;165;45;48;44;43;51;47;;0,0,0,1;0;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;215;-3713.611,173.9644;Inherit;False;2;0;FLOAT;1;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;218;-3439.377,-133.3693;Inherit;True;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;1,1,1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TexturePropertyNode;47;-3695.353,-2702.124;Inherit;True;Property;_AlphaOverride;Alpha Override;12;0;Create;False;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.TFHCRemapNode;219;-3168.295,-132.3623;Inherit;True;5;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;1,1,1;False;3;FLOAT3;0,0,0;False;4;FLOAT3;1,1,1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;424;-3756.214,885.551;Inherit;False;Property;_MiddlePointPos1;Middle Point Position 2;28;0;Create;False;0;0;0;False;0;False;0.5;0.5;-1;0.99;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector4Node;86;-632.0173,-2552.366;Inherit;False;Property;_DetailMultiplyChannel;Detail Multiply Channel;19;0;Create;True;0;0;0;False;0;False;0,0,0,0;0,0,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;206;-3626.621,-2151.139;Inherit;False;204;UVModifiers;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;43;-3348.505,-2565.63;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector2Node;51;-3302.505,-2415.63;Inherit;False;Property;_AlphaOverridePanning;Alpha Override Panning;14;0;Create;False;0;0;0;False;0;False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;363;-236.08,-2517.672;Inherit;True;2;2;0;FLOAT4;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;425;-3438.314,790.5511;Inherit;True;2;0;FLOAT3;1,0,0;False;1;FLOAT;1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;427;-3757.551,513.5864;Inherit;True;210;PreRamp;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;287;-3437.1,-2098.772;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PannerNode;44;-3001.505,-2558.63;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.BreakToComponentsNode;364;-42.31336,-2540.137;Inherit;False;FLOAT4;1;0;FLOAT4;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.Vector4Node;87;-306.9171,-3233.965;Inherit;False;Property;_DetailAdditiveChannel;Detail Additive Channel;21;0;Create;True;0;0;0;False;0;False;0,0,0,0;0,0,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;213;-3497.074,514.6613;Inherit;True;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.PannerNode;33;-3118.868,-2100.573;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.WireNode;300;-3289.022,-2074.835;Inherit;False;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.SamplerNode;45;-2782.733,-2691.905;Inherit;True;Property;_TextureSample2;Texture Sample 2;4;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.CommentaryNode;270;-1065.607,-1133.974;Inherit;False;1435.269;484.5625;Erosion;9;335;258;276;366;413;414;416;415;418;;0,0,0,1;0;0
Node;AmplifyShaderEditor.Vector4Node;48;-2721.733,-2502.905;Inherit;False;Property;_AlphaOverrideChannel;Alpha Override Channel;13;0;Create;False;0;0;0;False;0;False;0,0,0,1;0,0,0,1;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;157;55.47039,-3210.656;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.DynamicAppendNode;365;78.48648,-2540.239;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;392;-143.637,-2289.722;Inherit;False;Property;_MultiplyNoiseDesaturation;Multiply Noise Desaturation;20;0;Create;True;0;0;0;False;0;False;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;217;-3208.621,517.0002;Inherit;True;5;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;1,1,1;False;3;FLOAT3;0,0,0;False;4;FLOAT3;1,1,1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ColorNode;222;-3196.067,-336.8553;Inherit;False;Property;_WhiteColor;Highs;25;1;[HDR];Create;False;1;Color Ramping;0;0;False;0;False;1,0.8950032,0,0;1,0.8950032,0,0;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.ColorNode;223;-3126.628,108.6669;Inherit;False;Property;_MidColor;Middles;27;1;[HDR];Create;False;0;0;0;False;0;False;1,0.4447915,0,0;1,0.4447915,0,0;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.ColorNode;224;-2629.779,-7.544342;Inherit;False;Property;_LastColor;Lows;29;1;[HDR];Create;False;0;0;0;False;0;False;1,0,0,0;1,0,0,0;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.Vector4Node;12;-2623.607,-1905.958;Inherit;False;Property;_MainAlphaChannel;Main Alpha Channel;5;0;Create;True;0;0;0;False;0;False;0,0,0,1;0,0,0,1;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;165;-2394.869,-2692.418;Inherit;True;Channel Picker Alpha;-1;;205;e49841402b321534583d1dc019041b68;0;2;5;FLOAT4;1,0,0,0;False;7;FLOAT4;0,0,0,1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;28;-2707.533,-2092.719;Inherit;True;Property;_TextureSample1;Texture Sample 1;4;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.TextureCoordinatesNode;258;-1025.338,-881.0559;Inherit;False;0;-1;4;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector4Node;271;-31.55542,-2856.577;Inherit;False;Property;_DetailDisolveChannel;Detail Disolve Channel;22;0;Create;True;0;0;0;False;0;False;1,0,0,0;0,0,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DesaturateOpNode;362;225.7036,-2482.675;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.BreakToComponentsNode;156;198.2371,-3233.121;Inherit;False;FLOAT4;1;0;FLOAT4;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.RangedFloatNode;108;230.9095,-2341.567;Inherit;False;Constant;_Float0;Float 0;15;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;225;-2877.33,-181.9317;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;220;-2356.621,74.90042;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;212;-2109.021,307.2236;Inherit;True;210;PreRamp;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;105;-384.6793,-2526.739;Inherit;False;4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;49;-2074.432,-2478.706;Inherit;True;AlphaOverride;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;420;-762.884,-717.6194;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;-1;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;302;-981.6526,-610.3616;Inherit;False;1410.162;604.1428;Alpha Processing;10;313;314;52;198;201;203;199;55;202;53;;0,0,0,1;0;0
Node;AmplifyShaderEditor.FunctionNode;164;-2387.532,-2087.719;Inherit;False;Channel Picker Alpha;-1;;210;e49841402b321534583d1dc019041b68;0;2;5;FLOAT4;1,0,0,0;False;7;FLOAT4;0,0,0,1;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;413;-768.9469,-871.9285;Inherit;False;Constant;_Vector0;Vector 0;3;0;Create;True;0;0;0;False;0;False;-0.25,1;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.FunctionNode;360;208.4446,-2877.577;Inherit;False;Channel Picker;-1;;211;dc5f4cb24a8bdf448b40a1ec5866280e;0;2;5;FLOAT4;1,0,0,0;False;7;FLOAT4;0,0,0,1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ConditionalIfNode;106;399.5607,-2527.759;Inherit;False;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DynamicAppendNode;155;323.0369,-3236.223;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.LerpOp;226;-1837.334,256.5038;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.TFHCRemapNode;414;-560.9469,-967.9285;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;275;217.2316,-2785.336;Inherit;True;DisolveNoise;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;314;-877.2017,-560.2477;Inherit;False;Constant;_1;1;31;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;30;-2148.532,-2091.719;Inherit;True;MainAlpha;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;202;-673.6529,-106.2182;Inherit;False;Property;_SoftFadeFactor;Soft Fade Factor;10;0;Create;False;0;0;0;False;0;False;0.1;0.1;0.1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;52;-937.5291,-485.9505;Inherit;True;49;AlphaOverride;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.DesaturateOpNode;191;482.254,-3138.659;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;92;607.0869,-2550.99;Inherit;True;MultiplyNoise;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.BreakToComponentsNode;230;-1641.734,-9.896179;Inherit;False;COLOR;1;0;COLOR;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.WireNode;428;-1877.098,195.2045;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;415;-352.947,-823.9285;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.25;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;313;-724.6451,-511.4518;Inherit;False;Property;_UseAlphaOverride;Use Alpha Override;11;0;Create;False;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DepthFade;199;-390.653,-174.2183;Inherit;False;True;False;True;2;1;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;276;-1027.047,-1083.472;Inherit;True;275;DisolveNoise;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;53;-644.1548,-392.3482;Inherit;True;30;MainAlpha;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;98;-415.6464,-1559.49;Inherit;False;92;MultiplyNoise;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;91;680.1696,-3157.392;Inherit;True;AdditiveNoise;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DynamicAppendNode;231;-1506.534,2.103729;Inherit;True;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SaturateNode;203;-136.6527,-180.2186;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;55;-419.1545,-486.3481;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;416;-195.9471,-1081.928;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;303;396.9286,-1914.042;Inherit;False;870.6288;722.2654;Final Color Processing;6;37;136;39;71;236;234;;0,0,0,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;101;-177.1052,-1554.331;Inherit;False;91;AdditiveNoise;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;207;-383.9355,-1649.26;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;232;-1278.143,2.90301;Inherit;True;PostRamp;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SaturateNode;335;93.94887,-1086.522;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;418;185.9862,-846.3901;Inherit;False;Constant;_Float1;Float 1;32;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;201;4.409043,-360.273;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;234;455.677,-1566.246;Inherit;False;232;PostRamp;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleAddOpNode;102;-144.7843,-1651.257;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;71;766.7729,-1488.998;Inherit;False;0;-1;4;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StaticSwitch;417;348.166,-1022.293;Inherit;False;Property;_DisableErosion;Disable Erosion;35;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;198;135.6091,-436.4731;Inherit;True;Property;_UseSoftAlpha;Use Soft Particles?;9;0;Create;False;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;136;811.0246,-1326.777;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;236;430.0302,-1660.542;Inherit;False;Property;_UseRamp;Use Color Ramping?;24;0;Create;False;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT4;0,0,0,0;False;0;FLOAT4;0,0,0,0;False;2;FLOAT4;0,0,0,0;False;3;FLOAT4;0,0,0,0;False;4;FLOAT4;0,0,0,0;False;5;FLOAT4;0,0,0,0;False;6;FLOAT4;0,0,0,0;False;7;FLOAT4;0,0,0,0;False;8;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.VertexColorNode;37;790.1487,-1864.042;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;411;813.1597,-2057.002;Inherit;False;Property;_FresnelScale;Fresnel Scale;33;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;404;846.5385,-2158.126;Inherit;False;Property;_FresnelPower;Fresnel Power;32;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector4Node;432;581.3877,-2821.55;Inherit;False;Property;_DetailVertexOffsetChannel;Detail Vertex Offset Channel;23;0;Create;True;0;0;0;False;0;False;1,0,0,0;0,0,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;269;760.189,-1071.058;Inherit;True;2;2;0;FLOAT;1;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;61;789.1661,-844.8502;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;39;1031.557,-1664.882;Inherit;True;3;3;0;COLOR;1,1,1,0;False;1;FLOAT4;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;405;1108.238,-2145.026;Inherit;False;Property;_FresnelColor;Fresnel Color;34;1;[HDR];Create;True;0;0;0;False;0;False;1,1,1,1;1,1,1,1;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.FresnelNode;406;1073.443,-2375.809;Inherit;True;Standard;TangentNormal;ViewDir;False;True;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;433;950.3877,-2862.55;Inherit;False;Channel Picker;-1;;214;dc5f4cb24a8bdf448b40a1ec5866280e;0;2;5;FLOAT4;1,0,0,0;False;7;FLOAT4;0,0,0,1;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;410;1390.65,-2360.209;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;396;1008,-960;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;434;1189.046,-2841.912;Inherit;False;VertexOffset;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;409;1695.235,-2322.251;Inherit;True;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;435;1937.502,-1052.923;Inherit;False;434;VertexOffset;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;429;1939.455,-1221.914;Inherit;False;1;-1;4;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StaticSwitch;403;1849.538,-2016.326;Inherit;False;Property;_Fresnel;Fresnel;31;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;430;2176.779,-1120.864;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NormalVertexDataNode;437;2174.049,-934.0656;Inherit;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.BreakToComponentsNode;459;2064,-1904;Inherit;False;COLOR;1;0;COLOR;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;436;2390.049,-1174.065;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ClampOpNode;394;-2174.582,-1728.42;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;393;-2413.508,-1811.248;Inherit;False;Channel Picker;-1;;215;dc5f4cb24a8bdf448b40a1ec5866280e;0;2;5;FLOAT4;1,0,0,0;False;7;FLOAT4;0,0,0,1;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;366;-726.006,-1093.102;Inherit;False;Property;_UseAlphaDisolve;Use Alpha For Disolve;8;0;Create;False;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;391;2233.221,-2274.796;Inherit;False;Property;_SourceBlendRGB;Blend Mode;0;1;[Enum];Create;False;0;0;1;UnityEngine.Rendering.BlendMode;True;0;False;10;10;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;431;2217.206,-2198.228;Inherit;False;Property;_Culling;Culling;2;1;[Enum];Create;True;0;0;1;UnityEngine.Rendering.CullMode;True;0;False;1;2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;440;1728,-1728;Inherit;False;Property;_AlphaClip;Alpha Clip;1;0;Create;True;0;0;1;UnityEngine.Rendering.CompareFunction;True;0;False;0;0.47;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;460;2221.67,-1864.229;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;468;2560,-1952;Float;False;False;-1;2;ASEMaterialInspector;100;1;New Amplify Shader;e1de45c0d41f68c41b2cc20c8b9c05ef;True;ShadowCaster;0;3;ShadowCaster;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;RenderType=Opaque=RenderType;True;2;False;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;False;;True;3;False;;False;True;1;LightMode=ShadowCaster;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;467;2560,-1773;Float;False;False;-1;2;ASEMaterialInspector;100;7;New Amplify Shader;e1de45c0d41f68c41b2cc20c8b9c05ef;True;Deferred;0;2;Deferred;4;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;RenderType=Opaque=RenderType;True;2;False;0;False;True;0;1;False;;0;False;;0;1;False;;0;False;;True;0;False;;0;False;;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;1;LightMode=Deferred;True;2;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;466;2560,-1841;Float;False;False;-1;2;ASEMaterialInspector;100;7;New Amplify Shader;e1de45c0d41f68c41b2cc20c8b9c05ef;True;ForwardAdd;0;1;ForwardAdd;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;RenderType=Opaque=RenderType;True;2;False;0;False;True;4;1;False;;1;False;;0;1;False;;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;;False;False;True;1;LightMode=ForwardAdd;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;465;2560,-1952;Float;False;True;-1;2;ASEMaterialInspector;100;7;Piloto Studio/Opaque UberFX;e1de45c0d41f68c41b2cc20c8b9c05ef;True;ForwardBase;0;0;ForwardBase;3;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;2;True;_Culling;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;RenderType=TransparentCutout=RenderType;True;2;False;0;True;True;2;5;False;;10;True;_SourceBlendRGB;0;1;False;;0;False;;True;0;False;;0;False;;False;False;False;False;False;False;False;False;False;True;0;False;;True;True;0;True;_Culling;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;True;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;1;LightMode=ForwardBase;True;2;False;0;;0;0;Standard;0;0;4;True;True;True;True;False;;False;0
WireConnection;81;2;83;0
WireConnection;80;0;81;0
WireConnection;80;2;84;0
WireConnection;79;0;83;0
WireConnection;79;1;80;0
WireConnection;357;5;79;0
WireConnection;357;7;85;0
WireConnection;90;0;357;0
WireConnection;443;0;441;0
WireConnection;443;1;442;0
WireConnection;286;2;27;0
WireConnection;444;0;443;0
WireConnection;446;0;286;0
WireConnection;400;0;397;1
WireConnection;400;1;397;2
WireConnection;284;0;93;0
WireConnection;284;1;95;0
WireConnection;447;0;444;0
WireConnection;447;1;445;0
WireConnection;448;0;446;1
WireConnection;449;1;445;0
WireConnection;450;0;446;0
WireConnection;450;1;444;0
WireConnection;401;0;284;0
WireConnection;401;1;400;0
WireConnection;451;0;450;0
WireConnection;451;1;447;0
WireConnection;452;0;448;0
WireConnection;452;1;449;0
WireConnection;402;1;284;0
WireConnection;402;0;401;0
WireConnection;453;0;446;1
WireConnection;453;1;452;0
WireConnection;454;0;446;0
WireConnection;454;1;451;0
WireConnection;204;0;402;0
WireConnection;455;0;454;0
WireConnection;455;1;453;0
WireConnection;456;1;286;0
WireConnection;456;0;455;0
WireConnection;288;0;456;0
WireConnection;288;1;205;0
WireConnection;22;0;288;0
WireConnection;22;2;23;0
WireConnection;299;0;27;0
WireConnection;6;0;299;0
WireConnection;6;1;22;0
WireConnection;379;0;10;0
WireConnection;376;0;6;0
WireConnection;380;0;376;0
WireConnection;380;1;379;0
WireConnection;381;0;376;1
WireConnection;381;1;379;1
WireConnection;382;0;376;2
WireConnection;382;1;379;2
WireConnection;383;0;376;3
WireConnection;383;1;379;3
WireConnection;375;0;380;0
WireConnection;375;1;381;0
WireConnection;375;2;382;0
WireConnection;375;3;383;0
WireConnection;25;0;375;0
WireConnection;166;0;36;0
WireConnection;166;1;167;0
WireConnection;210;0;166;0
WireConnection;395;0;426;0
WireConnection;395;1;214;0
WireConnection;215;1;214;0
WireConnection;218;0;395;0
WireConnection;219;0;218;0
WireConnection;219;2;215;0
WireConnection;43;2;47;0
WireConnection;363;0;86;0
WireConnection;363;1;79;0
WireConnection;425;0;219;0
WireConnection;425;1;424;0
WireConnection;287;0;206;0
WireConnection;287;1;456;0
WireConnection;44;0;43;0
WireConnection;44;2;51;0
WireConnection;364;0;363;0
WireConnection;213;0;427;0
WireConnection;213;1;425;0
WireConnection;33;0;287;0
WireConnection;33;2;23;0
WireConnection;300;0;27;0
WireConnection;45;0;47;0
WireConnection;45;1;44;0
WireConnection;157;0;87;0
WireConnection;157;1;79;0
WireConnection;365;0;364;0
WireConnection;365;1;364;1
WireConnection;365;2;364;2
WireConnection;365;3;364;3
WireConnection;217;0;213;0
WireConnection;217;2;215;0
WireConnection;165;5;45;0
WireConnection;165;7;48;0
WireConnection;28;0;300;0
WireConnection;28;1;33;0
WireConnection;362;0;365;0
WireConnection;362;1;392;0
WireConnection;156;0;157;0
WireConnection;225;0;223;0
WireConnection;225;1;222;0
WireConnection;225;2;219;0
WireConnection;220;0;224;0
WireConnection;220;1;223;0
WireConnection;220;2;217;0
WireConnection;105;0;86;1
WireConnection;105;1;86;2
WireConnection;105;2;86;3
WireConnection;105;3;86;4
WireConnection;49;0;165;0
WireConnection;420;0;258;4
WireConnection;164;5;28;0
WireConnection;164;7;12;0
WireConnection;360;5;79;0
WireConnection;360;7;271;0
WireConnection;106;0;105;0
WireConnection;106;2;362;0
WireConnection;106;3;108;0
WireConnection;106;4;108;0
WireConnection;155;0;156;0
WireConnection;155;1;156;1
WireConnection;155;2;156;2
WireConnection;155;3;156;3
WireConnection;226;0;220;0
WireConnection;226;1;225;0
WireConnection;226;2;212;0
WireConnection;414;0;420;0
WireConnection;414;3;413;1
WireConnection;414;4;413;2
WireConnection;275;0;360;0
WireConnection;30;0;164;0
WireConnection;191;0;155;0
WireConnection;92;0;106;0
WireConnection;230;0;226;0
WireConnection;428;0;212;0
WireConnection;415;0;414;0
WireConnection;313;1;314;0
WireConnection;313;0;52;0
WireConnection;199;0;202;0
WireConnection;91;0;191;0
WireConnection;231;0;230;0
WireConnection;231;1;230;1
WireConnection;231;2;230;2
WireConnection;231;3;428;0
WireConnection;203;0;199;0
WireConnection;55;0;313;0
WireConnection;55;1;53;0
WireConnection;416;0;276;0
WireConnection;416;1;414;0
WireConnection;416;2;415;0
WireConnection;207;0;166;0
WireConnection;207;1;98;0
WireConnection;232;0;231;0
WireConnection;335;0;416;0
WireConnection;201;0;55;0
WireConnection;201;1;203;0
WireConnection;102;0;207;0
WireConnection;102;1;101;0
WireConnection;417;1;335;0
WireConnection;417;0;418;0
WireConnection;198;1;55;0
WireConnection;198;0;201;0
WireConnection;136;0;71;3
WireConnection;236;1;102;0
WireConnection;236;0;234;0
WireConnection;269;0;417;0
WireConnection;269;1;198;0
WireConnection;39;0;37;0
WireConnection;39;1;236;0
WireConnection;39;2;136;0
WireConnection;406;2;411;0
WireConnection;406;3;404;0
WireConnection;433;5;79;0
WireConnection;433;7;432;0
WireConnection;410;0;39;0
WireConnection;410;1;405;0
WireConnection;410;2;406;0
WireConnection;396;0;269;0
WireConnection;396;1;61;4
WireConnection;434;0;433;0
WireConnection;409;0;396;0
WireConnection;409;1;410;0
WireConnection;403;1;39;0
WireConnection;403;0;409;0
WireConnection;430;0;429;3
WireConnection;430;1;435;0
WireConnection;459;0;403;0
WireConnection;436;0;430;0
WireConnection;436;1;437;0
WireConnection;394;0;393;0
WireConnection;393;5;6;0
WireConnection;393;7;10;0
WireConnection;366;1;276;0
WireConnection;366;0;198;0
WireConnection;460;0;459;0
WireConnection;460;1;459;1
WireConnection;460;2;459;2
WireConnection;465;0;460;0
WireConnection;465;1;396;0
WireConnection;465;2;436;0
ASEEND*/
//CHKSM=CCF886BE37082D550810EC98121F9D725DC7AAD8