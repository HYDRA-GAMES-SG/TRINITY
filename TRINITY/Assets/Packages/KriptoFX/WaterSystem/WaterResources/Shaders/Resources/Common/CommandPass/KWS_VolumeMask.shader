Shader "Hidden/KriptoFX/KWS/ClipMask"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" { }
	}
	SubShader
	{
		Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
		

		HLSLINCLUDE

		#include "../../PlatformSpecific/Includes/KWS_HelpersIncludes.cginc"
		
		DECLARE_TEXTURE(_SourceRT);
		float4 _SourceRTHandleScale;
		float4 _SourceRT_TexelSize;
		float KWS_WaterLevel;
			

		struct vertexInput
		{
			uint vertexID : SV_VertexID;
			UNITY_VERTEX_INPUT_INSTANCE_ID
		};

		struct vertexOutput
		{
			float4 vertex : SV_POSITION;
			float2 uv : TEXCOORD0;
			UNITY_VERTEX_OUTPUT_STEREO
		};

		vertexOutput vertHorizon(vertexInput v)
			{
				vertexOutput o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				o.vertex = GetTriangleVertexPosition(v.vertexID);
				o.uv = GetTriangleUVScaled(v.vertexID);
				return o;
			}

		ENDHLSL

		//ocean underwater mask
		Pass
		{
			Cull Off
		
			HLSLPROGRAM
			#pragma vertex vertHorizon
			#pragma fragment fragHorizon
			#pragma target 4.6

			
			struct FragmentOutput
			{
				half4 mask : SV_Target0;
				half id : SV_Target1;
			};

			FragmentOutput fragHorizon(vertexOutput i) : SV_Target
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
				FragmentOutput o = (FragmentOutput)0;

				float3 worldPos = GetWorldSpacePositionFromDepth(i.uv, 0.0);
				float oceanUnderwaterMask = worldPos.y <= KWS_WaterLevel ? 1 : 0;
				o.mask = half4(oceanUnderwaterMask, 0, 0, 0);
				o.id = oceanUnderwaterMask * KWS_WATER_MASK_DECODING_VALUE;

				return o;
			}
			

			ENDHLSL
		}
			
		
		////sdf front/back depth
		//Pass
		//{
		//	Cull Off

		//	HLSLPROGRAM
		//	#pragma vertex vertHorizon
		//	#pragma fragment fragHorizon
		//	#pragma target 4.6

		//	DECLARE_TEXTURE(_SourceRT);
		//	float4 _SourceRTHandleScale;
		//	float4 _SourceRT_TexelSize;
			

		//	struct vertexInput
		//	{
		//		uint vertexID : SV_VertexID;
		//		UNITY_VERTEX_INPUT_INSTANCE_ID
		//	};

		//	struct vertexOutput
		//	{
		//		float4 vertex : SV_POSITION;
		//		float2 uv : TEXCOORD0;
		//		UNITY_VERTEX_OUTPUT_STEREO
		//	};
			
		//	struct FragmentOutput
		//	{
		//		float2 depth : SV_Target0;
		//		half id : SV_Target1;
		//	};

		//	vertexOutput vertHorizon(vertexInput v)
		//	{
		//		vertexOutput o;
		//		UNITY_SETUP_INSTANCE_ID(v);
		//		UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
		//		o.vertex = GetTriangleVertexPosition(v.vertexID);
		//		o.uv = GetTriangleUVScaled(v.vertexID);
		//		return o;
		//	}

		//	FragmentOutput fragHorizon(vertexOutput i) : SV_Target
		//	{
		//		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
		//		FragmentOutput o = (FragmentOutput)0;

		//		float2 screenUV = i.uv;
		//		float3 startPos = GetCameraAbsolutePosition();
		//		float sceneDepth = GetSceneDepth(screenUV);
		//		float3 worldPos = GetWorldSpacePositionFromDepth(screenUV, 0);
		//		float3 rayDir =  normalize(worldPos - startPos);

		//		uint waterID = GetWaterSurfaceID(screenUV);
		//		float waterMask =  GetWaterMask(screenUV);
		//		float maskTop = waterMask.x > 0.7 && waterMask < 0.8;
		//		float maskBot =  waterMask > 0.45 && waterMask < 0.55;
				

		//		//uint jumpFloodID = GetJumpFloodVolumeMask(screenUV) ;
				
		//		float waterDepth = GetWaterSurfaceDepth(screenUV);
		//		float3 worldWaterDepth  = GetWorldSpacePositionFromDepth(screenUV, waterDepth);
		//		float distanceToWaterSurface = length(worldWaterDepth - startPos);
		//		if(waterDepth == 0) distanceToWaterSurface = 0;
				
		//		float2 result = 1000000;
		//		float id = 0;

		//		//WaterVolumeData data = KWS_WaterVolumeDataBuffer[0];
		//		//float3 box = KWS_IntersectionBoxWithSDF(startPos - data.Position.xyz, rayDir, (float3x3)data.Rotation, data.Size.xyz); 
		//		//box.x = max(box.x, 0.0001);
		//		//if(box.x < box.y && box.y > 0.0 && (waterID == data.InstanceID || waterID == 0))
		//		//{
					
		//		//	if(waterMask.x > 0.7) box.x = distanceToWaterSurface * maskTop;
		//		//	else if(waterMask.x > 0.0) box.y = min(box.y, distanceToWaterSurface);
				
		//		//	result = box.xy;
		//		//	id = data.InstanceID;
		//		//}

				
		//		for(uint idx = 0; idx < KWS_WaterVolumeDataBufferLength; idx++)
		//		{
		//			WaterVolumeData data = KWS_WaterVolumeDataBuffer[idx];
		//			float3 box = KWS_IntersectionBoxWithSDF(startPos - data.Position.xyz, rayDir, (float3x3)data.Rotation, data.Size.xyz); 
		//			box.x = max(box.x, 0.0001);

		//			//if(box.x < box.y && box.y > 0.0 && (waterID == data.InstanceID || waterID == 0))
		//			if(box.x < box.y && box.y > 0.0)
		//			{
		//				if(waterMask.x > 0.7) box.x = distanceToWaterSurface * maskTop;
		//				else if(waterMask.x > 0.0) box.y = min(box.y, distanceToWaterSurface);

		//				if(box.x < box.y)
		//				{	
		//					if(box.x < result.x) id = data.InstanceID;
		//					result.x = min(result.x, box.x);
		//					result.y = min(result.y, box.y);
		//				}
		//			}
		//		}
				
		//		if(result.x > 0)
		//		{
		//			float4 clipPos = UnityWorldToClipPos(startPos + result.x * rayDir);
		//			float frontZ = saturate(clipPos.z / clipPos.w);
		//			clipPos = UnityWorldToClipPos(startPos + result.y * rayDir);
		//			float backZ = saturate(clipPos.z / clipPos.w);
		//			result = float2(frontZ, backZ);

		//			o.depth = result;
		//			o.id = id * KWS_WATER_MASK_DECODING_VALUE;
		//		}
				
		//		return o;
		//	}
			

		//	ENDHLSL
		//}
	}
}