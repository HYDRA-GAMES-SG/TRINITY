Shader "Hidden/KriptoFX/KWS/WaterTesselated"
{
	Properties
	{
		srpBatcherFix ("srpBatcherFix", Float) = 0
		[HideInInspector]KWS_StencilMaskValue("KWS_StencilMaskValue", Int) = 32
	}

	SubShader
	{
		Tags { "Queue" = "Transparent-1" "IgnoreProjector" = "True" "RenderType" = "Transparent" "DisableBatching" = "true" }
	
		Stencil
		{
			Ref [KWS_StencilMaskValue]
            ReadMask [KWS_StencilMaskValue]
			Comp Greater
			Pass keep
		}

		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite On
			Cull Back

			HLSLPROGRAM
			
			#pragma shader_feature  KW_FLOW_MAP_EDIT_MODE

			#pragma multi_compile _ USE_WATER_INSTANCING
			#pragma multi_compile _ KW_FLOW_MAP KW_FLOW_MAP_FLUIDS
			#pragma multi_compile _ KW_DYNAMIC_WAVES
			#pragma multi_compile _ USE_SHORELINE

 			#pragma multi_compile_fragment _ REFLECT_SUN
			#pragma multi_compile_fragment _ KWS_USE_VOLUMETRIC_LIGHT
			#pragma multi_compile_fragment _ KWS_SSR_REFLECTION
			#pragma multi_compile_fragment _ KWS_USE_PLANAR_REFLECTION
			#pragma multi_compile_fragment _ KWS_USE_AQUARIUM_RENDERING

			#include "../PlatformSpecific/Includes/KWS_VertFragIncludes.cginc"
			#include "KWS_Tessellation.cginc"
			
			#pragma vertex vertHull
			#pragma fragment fragWater
			#pragma hull HS
			#pragma domain DS
			#pragma target 4.6
			#pragma editor_sync_compilation

			ENDHLSL
		}
	}
}