// Made with Amplify Shader Editor v1.9.6.3
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Piloto Studio/PBR_UberShader"
{
	Properties
	{
		_BaseColorMap("Base Color Map", 2D) = "white" {}
		_Emission("Emission", 2D) = "white" {}
		_AdditiveMaskEdge("Additive Mask Edge", Range( 0 , 1)) = 1
		[HDR]_EmissiveTint("Emissive Tint", Color) = (1,1,1,1)
		_Intensity("Intensity", Float) = 0
		_PanningMask("Panning Mask", 2D) = "white" {}
		_PanningSpeed("Panning Speed", Vector) = (0,0,0,0)
		_PanningNoiseStrenght("Panning Noise Strenght", Float) = 0.5
		_Normal("Normal", 2D) = "white" {}
		_NormalScale("Normal Scale", Range( 0 , 2)) = 1
		_OcclusionRoughtMetal("Occlusion Rought Metal", 2D) = "white" {}
		_OcclusionScale("Occlusion Scale", Range( 0 , 1)) = 0
		_SmoothnessScale("Smoothness Scale", Range( 0 , 1)) = 1
		_MetallicScale("Metallic Scale", Range( 0 , 1)) = 1
		_BaseColorTint("BaseColor Tint", Color) = (1,1,1,0)
		_AlphaClipping("Alpha Clipping", Float) = 1
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#include "UnityStandardUtils.cginc"
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D _Normal;
		uniform float4 _Normal_ST;
		uniform float _NormalScale;
		uniform float4 _BaseColorTint;
		uniform sampler2D _BaseColorMap;
		uniform float4 _BaseColorMap_ST;
		uniform float _AdditiveMaskEdge;
		uniform sampler2D _Emission;
		uniform float4 _Emission_ST;
		uniform float4 _EmissiveTint;
		uniform float _PanningNoiseStrenght;
		uniform sampler2D _PanningMask;
		uniform float2 _PanningSpeed;
		uniform float4 _PanningMask_ST;
		uniform float _Intensity;
		uniform float _MetallicScale;
		uniform sampler2D _OcclusionRoughtMetal;
		uniform float4 _OcclusionRoughtMetal_ST;
		uniform float _SmoothnessScale;
		uniform float _OcclusionScale;
		uniform float _AlphaClipping;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_Normal = i.uv_texcoord * _Normal_ST.xy + _Normal_ST.zw;
			o.Normal = UnpackScaleNormal( tex2D( _Normal, uv_Normal ), _NormalScale );
			float2 uv_BaseColorMap = i.uv_texcoord * _BaseColorMap_ST.xy + _BaseColorMap_ST.zw;
			float4 tex2DNode1 = tex2D( _BaseColorMap, uv_BaseColorMap );
			o.Albedo = ( _BaseColorTint * tex2DNode1 ).rgb;
			float2 uv_Emission = i.uv_texcoord * _Emission_ST.xy + _Emission_ST.zw;
			float4 tex2DNode2 = tex2D( _Emission, uv_Emission );
			float2 uv_PanningMask = i.uv_texcoord * _PanningMask_ST.xy + _PanningMask_ST.zw;
			float2 panner25 = ( 1.0 * _Time.y * _PanningSpeed + uv_PanningMask);
			o.Emission = ( ( ( ( _AdditiveMaskEdge * ( tex2DNode2 * _EmissiveTint ) ) * 0.5 ) + ( _PanningNoiseStrenght * ( tex2DNode2 * ( _EmissiveTint * tex2D( _PanningMask, panner25 ).r ) ) ) ) * _Intensity ).rgb;
			float2 uv_OcclusionRoughtMetal = i.uv_texcoord * _OcclusionRoughtMetal_ST.xy + _OcclusionRoughtMetal_ST.zw;
			float4 tex2DNode4 = tex2D( _OcclusionRoughtMetal, uv_OcclusionRoughtMetal );
			o.Metallic = ( _MetallicScale * tex2DNode4.b );
			o.Smoothness = ( saturate( ( 1.0 - tex2DNode4.g ) ) * _SmoothnessScale );
			o.Occlusion = ( tex2DNode4.r * _OcclusionScale );
			o.Alpha = 1;
			clip( tex2DNode1.a - _AlphaClipping );
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=19603
Node;AmplifyShaderEditor.Vector2Node;26;-1258,1443.5;Inherit;False;Property;_PanningSpeed;Panning Speed;6;0;Create;True;0;0;0;False;0;False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.TextureCoordinatesNode;28;-1305,1219.5;Inherit;False;0;24;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PannerNode;25;-1077,1391.5;Inherit;False;3;0;FLOAT2;1,1;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ColorNode;22;-796,1133;Inherit;False;Property;_EmissiveTint;Emissive Tint;3;1;[HDR];Create;True;0;0;0;False;0;False;1,1,1,1;0,0,0,1;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SamplerNode;2;-891,935.5;Inherit;True;Property;_Emission;Emission;1;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SamplerNode;24;-875,1369.5;Inherit;True;Property;_PanningMask;Panning Mask;5;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;27;-557,1267.5;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;30;-572,940.4998;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;33;-736,773.4998;Inherit;False;Property;_AdditiveMaskEdge;Additive Mask Edge;2;0;Create;True;0;0;0;False;0;False;1;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;38;-499,632.4998;Inherit;False;Constant;_Float0;Float 0;10;0;Create;True;0;0;0;False;0;False;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;23;-391,939.4998;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;32;-448,761.4998;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;41;-588,1101.5;Inherit;False;Property;_PanningNoiseStrenght;Panning Noise Strenght;7;0;Create;True;0;0;0;False;0;False;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;4;-1179,223.5;Inherit;True;Property;_OcclusionRoughtMetal;Occlusion Rought Metal;10;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;37;-321,624.4998;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;39;-249,846.4998;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.OneMinusNode;17;-406,358.5;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;31;-125,668.4998;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SaturateNode;43;-209.7097,362.6552;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;46;-1248.957,30.83328;Inherit;False;Property;_NormalScale;Normal Scale;9;0;Create;True;0;0;0;False;0;False;1;0;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;48;-1072.957,431.8333;Inherit;False;Property;_OcclusionScale;Occlusion Scale;11;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;51;-360.957,446.8333;Inherit;False;Property;_SmoothnessScale;Smoothness Scale;12;0;Create;True;0;0;0;False;0;False;1;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;54;-393.957,69.83328;Inherit;False;Property;_MetallicScale;Metallic Scale;13;0;Create;True;0;0;0;False;0;False;1;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;1;-732,-269.5;Inherit;True;Property;_BaseColorMap;Base Color Map;0;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.ColorNode;58;-439.577,-466.1308;Inherit;False;Property;_BaseColorTint;BaseColor Tint;14;0;Create;True;0;0;0;False;0;False;1,1,1,0;1,1,1,0;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.RangedFloatNode;19;-80,784;Inherit;False;Property;_Intensity;Intensity;4;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;52;-46.95703,386.8333;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;47;-607.957,182.8333;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;55;-75.95703,93.83328;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;3;-781,-22.5;Inherit;True;Property;_Normal;Normal;8;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;True;white;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;59;-87.82632,-283.0322;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;62;77.38855,677.939;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;60;64,496;Inherit;False;Property;_AlphaClipping;Alpha Clipping;15;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;61;368,192;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;Piloto Studio/PBR_UberShader;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Custom;0.5;True;True;0;True;TransparentCutout;;Geometry;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;True;_AlphaClipping;0;0;0;False;0.1;False;;0;False;;False;17;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;16;FLOAT4;0,0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;25;0;28;0
WireConnection;25;2;26;0
WireConnection;24;1;25;0
WireConnection;27;0;22;0
WireConnection;27;1;24;1
WireConnection;30;0;2;0
WireConnection;30;1;22;0
WireConnection;23;0;2;0
WireConnection;23;1;27;0
WireConnection;32;0;33;0
WireConnection;32;1;30;0
WireConnection;37;0;32;0
WireConnection;37;1;38;0
WireConnection;39;0;41;0
WireConnection;39;1;23;0
WireConnection;17;0;4;2
WireConnection;31;0;37;0
WireConnection;31;1;39;0
WireConnection;43;0;17;0
WireConnection;52;0;43;0
WireConnection;52;1;51;0
WireConnection;47;0;4;1
WireConnection;47;1;48;0
WireConnection;55;0;54;0
WireConnection;55;1;4;3
WireConnection;3;5;46;0
WireConnection;59;0;58;0
WireConnection;59;1;1;0
WireConnection;62;0;31;0
WireConnection;62;1;19;0
WireConnection;61;0;59;0
WireConnection;61;1;3;0
WireConnection;61;2;62;0
WireConnection;61;3;55;0
WireConnection;61;4;52;0
WireConnection;61;5;47;0
WireConnection;61;10;1;4
ASEEND*/
//CHKSM=5CC327A45E677C1D19C4B3BCFF065F91AD8C4520