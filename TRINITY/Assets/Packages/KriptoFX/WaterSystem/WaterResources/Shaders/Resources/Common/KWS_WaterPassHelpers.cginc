#ifndef KWS_WATER_PASS_HELPERS

#define KWS_WATER_PASS_HELPERS

#ifndef KWS_WATER_VARIABLES
	#include "KWS_WaterVariables.cginc"
#endif

#ifndef KWS_COMMON_HELPERS
	#include "../Common/KWS_CommonHelpers.cginc"
#endif

#ifndef KWS_PLATFORM_SPECIFIC_HELPERS
	#include "../PlatformSpecific/KWS_PlatformSpecificHelpers.cginc"
#endif

float CalcMipLevel(float2 uv)
{
	float2 dx = ddx(uv);
	float2 dy = ddy(uv);
	float delta = max(dot(dx, dx), dot(dy, dy));
	return max(0.0, 0.5 * log2(delta));
}


//////////////////////////////////////////////    FFT_Waves_Pass    //////////////////////////////////////////////
#define MAX_FFT_WAVES_MAX_CASCADES 4

float KWS_WavesDomainSizes[MAX_FFT_WAVES_MAX_CASCADES];
float KWS_WavesDomainVisiableArea[MAX_FFT_WAVES_MAX_CASCADES];
float KWS_WindSpeed;
float KWS_WavesAreaScale;
float KWS_WavesCascades;

Texture2DArray KWS_FftWavesDisplace;
Texture2DArray KWS_FftWavesNormal;
SamplerState sampler_KWS_FftWavesNormal;
float4 KWS_FftWavesDisplace_TexelSize;
float4 KWS_FftWavesNormal_TexelSize;


inline float GetDomainSize(uint idx)
{
	return KWS_WavesDomainSizes[idx] * KWS_WavesAreaScale;
}

inline float GetDomainSize(uint idx, uint waterID)
{
	return KWS_WavesDomainSizes[idx] * KWS_WavesAreaScaleArray[waterID];
}

inline float GetDomainVisibleArea(uint idx)
{
	return KWS_WavesDomainVisiableArea[idx] * KWS_WavesAreaScale;
}


inline float GetFftFade(float distanceToCamera, int lodIdx, float farDistanceMinFade = 0.0)
{
	if (lodIdx == KWS_WavesCascades - 1)
	{
		float farDist = max(500, KW_WaterFarDistance * 0.5);
		return saturate(1.0 + farDistanceMinFade - saturate(distanceToCamera / farDist));
	}
	else
	{
		float fadeLod = saturate(distanceToCamera / GetDomainVisibleArea(lodIdx));
		fadeLod = 1 - fadeLod * fadeLod * fadeLod;
		return fadeLod;
	}
}

float3 GetFftWavesDisplacementLast(float3 worldPos)
{
	worldPos += KWS_WaterWorldPosOffset;
	int lastCascadeIdx = max(0, KWS_WavesCascades - 1);
	return KWS_FftWavesDisplace.SampleLevel(sampler_linear_repeat, float3(worldPos.xz / GetDomainSize(lastCascadeIdx), lastCascadeIdx), 0).xyz * KWS_WavesAreaScale;
}

float3 GetFftWavesDisplacementDetailsHQ(float3 worldPos)
{
	worldPos += KWS_WaterWorldPosOffset;
	float3 disp = Texture2DArraySampleLevelBicubic(KWS_FftWavesDisplace, sampler_linear_repeat, worldPos.xz / GetDomainSize(0), KWS_FftWavesDisplace_TexelSize, 0, 0).xyz;
	if (KWS_WavesCascades > 1) disp += Texture2DArraySampleLevelBicubic(KWS_FftWavesDisplace, sampler_linear_repeat, worldPos.xz / GetDomainSize(1), KWS_FftWavesDisplace_TexelSize, 1, 0).xyz;
	return disp * KWS_WavesAreaScale;
}

float3 GetFftWavesDisplacementDetails(float3 worldPos)
{
	worldPos += KWS_WaterWorldPosOffset;
	float3 disp = KWS_FftWavesDisplace.SampleLevel(sampler_linear_repeat, float3(worldPos.xz / GetDomainSize(0), 0), 0).xyz;
	if (KWS_WavesCascades > 1) disp += KWS_FftWavesDisplace.SampleLevel(sampler_linear_repeat, float3(worldPos.xz / GetDomainSize(1), 1), 0).xyz;
	return disp * KWS_WavesAreaScale;
}

float3 GetFftWavesDisplacementBuoyancy(float3 worldPos)
{
	worldPos += KWS_WaterWorldPosOffset;
	int minCascade = max(0, KWS_WavesCascades - 3);

	float3 finalData = 0;
	for (int idx = KWS_WavesCascades - 1; idx >= 0; idx--)
	{
		finalData += KWS_FftWavesDisplace.SampleLevel(sampler_linear_repeat, float3(worldPos.xz / GetDomainSize(idx), idx), 0).xyz;
	}
	return finalData * KWS_WavesAreaScale;
}

float3 GetFftWavesDisplacement(float3 worldPos)
{
	float distanceToCamera = GetWorldToCameraDistance(worldPos);
	float3 finalData = 0;
	worldPos += KWS_WaterWorldPosOffset;

	UNITY_LOOP for (int idx = KWS_WavesCascades - 1; idx >= 0; idx--)
	{
		float fade = GetFftFade(distanceToCamera, idx);
		if (fade < 0.01) continue;

		finalData += fade * KWS_FftWavesDisplace.SampleLevel(sampler_linear_repeat, float3(worldPos.xz / GetDomainSize(idx), idx), 0).xyz;
	}
	
	return finalData * KWS_WavesAreaScale;
}


float3 GetFftWavesNormalLod(float3 worldPos, float lod)
{
	float distanceToCamera = GetWorldToCameraDistance(worldPos);
	float3 finalData = float3(0, 1, 0);
	worldPos += KWS_WaterWorldPosOffset;

	UNITY_LOOP for (int idx = KWS_WavesCascades - 1; idx >= 0; idx--)
	{
		float fade = GetFftFade(distanceToCamera, idx, 0.25);
		if (fade < 0.01) continue;

		float3 data = fade * KWS_FftWavesNormal.SampleLevel(sampler_trilinear_repeat, float3(worldPos.xz / GetDomainSize(idx), idx), lod).xyz;
		data.y = 1;
		finalData = KWS_BlendNormals(finalData, data);
	}
	
	return float3(finalData.x, 1, finalData.z);
}



float3 GetFftWavesNormalFoam(float3 worldPos)
{
	float distanceToCamera = GetWorldToCameraDistance(worldPos);
	float3 finalData = float3(0, 1, 0);
	float foam = 0;
	int idx = KWS_WavesCascades;
	worldPos += KWS_WaterWorldPosOffset;

	UNITY_UNROLL for (int i = 0; i <= MAX_FFT_WAVES_MAX_CASCADES; i++)
	{
		idx--;
		if (idx < 0) break;

		float fade = GetFftFade(distanceToCamera, idx, 0.25);
		if (fade < 0.01) continue;
		
		float3 data;

		UNITY_BRANCH
		if (KWS_UseFilteredNormals == 1 && idx == 0) data = fade * Texture2DArraySampleBicubic(KWS_FftWavesNormal, sampler_KWS_FftWavesNormal, worldPos.xz / GetDomainSize(idx), KWS_FftWavesDisplace_TexelSize, idx).xyz;
		else
		{
			data = fade * KWS_FftWavesNormal.Sample(sampler_KWS_FftWavesNormal, float3(worldPos.xz / GetDomainSize(idx), idx)).xyz;
		}
		
		foam += data.y;
		data.y = 1;
		finalData = KWS_BlendNormals(finalData, data);
	}
	
	//return finalData;
	return float3(finalData.x, foam, finalData.z);
}


//////////////////////////////////////////////////////////////////////////////////////////////////////////////////




//////////////////////////////////////////////    PrePass (Mask/Normal/SSS/VolumeMask/Depth)   //////////////////////////////////////////////


//#define WATER_MASK_PASS_UNDERWATER_THRESHOLD 0.35
#define WATER_VOLUME_PRE_PASS_MAX_VALUE 1000000
#define WATER_LINE_HEIGHT_ENCODE_VALUE 10000

DECLARE_TEXTURE(KWS_WaterPrePassRT0);
DECLARE_TEXTURE(KWS_WaterPrePassRT1);
DECLARE_TEXTURE(KWS_WaterDepthRT);
DECLARE_TEXTURE(KWS_WaterIntersectionHalfLineTensionMaskRT);
float4 KWS_WaterPrePassRT0_TexelSize;
float4 KWS_WaterPrePass_RTHandleScale;

DECLARE_TEXTURE(KWS_WaterBackfacePrePassRT0);
DECLARE_TEXTURE(KWS_WaterBackfacePrePassRT1);
DECLARE_TEXTURE(KWS_WaterBackfaceDepthRT);

float4 KWS_WaterBackfacePrePassRT0_TexelSize;
float4 KWS_WaterBackfacePrePass_RTHandleScale;

inline float2 GetWaterPrePassUV(float2 uv)
{
	uv = GetRTHandleUV(uv, KWS_WaterPrePassRT0_TexelSize.xy, 1.0, KWS_WaterPrePass_RTHandleScale.xy);
	return uv;
}

inline float2 GetWaterBackfacePrePassUV(float2 uv)
{
	uv = GetRTHandleUV(uv, KWS_WaterBackfacePrePassRT0_TexelSize.xy, 1.0, KWS_WaterBackfacePrePass_RTHandleScale.xy);
	return uv;
}

inline float GetWaterSSS(float2 uv)
{
	return SAMPLE_TEXTURE_LOD(KWS_WaterPrePassRT0, sampler_linear_clamp, GetWaterPrePassUV(uv), 0).z;
}

inline float3 GetWaterNormals(float2 uv)
{
	float2 rawNormal = SAMPLE_TEXTURE_LOD(KWS_WaterPrePassRT1, sampler_linear_clamp, GetWaterPrePassUV(uv), 0).xy;
	#ifdef KWS_USE_AQUARIUM_RENDERING
		float2 rawNormalBackface = SAMPLE_TEXTURE_LOD(KWS_WaterBackfacePrePassRT1, sampler_point_clamp, GetWaterBackfacePrePassUV(uv), 0).xy;
		rawNormal += rawNormalBackface;
	#endif
	
	return float3(rawNormal.x, 1, rawNormal.y);
}


inline float GetWaterAquariumBackfaceMask(float2 uv)
{
	return SAMPLE_TEXTURE_LOD(KWS_WaterBackfacePrePassRT0, sampler_point_clamp, GetWaterBackfacePrePassUV(uv), 0).y;
}

//inside = 1, surface outside = 0.5, box fringe = 0.1
inline float GetWaterMask(float2 uv)
{
	#ifdef KWS_SHARED_API_INCLUDED
		return SAMPLE_TEXTURE_LOD(KWS_WaterPrePassRT0, sampler_point_clamp, GetWaterPrePassUV(uv), 0).y;
	#else
		float4 mask = SAMPLE_TEXTURE_GATHER_GREEN(KWS_WaterPrePassRT0, sampler_point_clamp, GetWaterPrePassUV(uv));
		return max(mask.x, max(mask.y, max(mask.z, mask.w)));
	#endif

	//float mask = SAMPLE_TEXTURE_LOD(KWS_WaterPrePassRT0, sampler_point_clamp, GetWaterPrePassUV(uv), 0).y;
	
	//float center = SAMPLE_TEXTURE_LOD(KW_WaterMaskScatterNormals, sampler_point_clamp, GetWaterPrePassUV(uv), 0).x;
	//float up = SAMPLE_TEXTURE_LOD_OFFSET(KW_WaterMaskScatterNormals, sampler_point_clamp, GetWaterPrePassUV(uv), 0, int2(0, 1)).x;
	//float down = SAMPLE_TEXTURE_LOD_OFFSET(KW_WaterMaskScatterNormals, sampler_point_clamp, GetWaterPrePassUV(uv), 0, int2(0, -1)).x;
	//float diff = (up + down) * 0.5 - center;

	//if((center == 0.0 || center == 1.0) && down == up) return down;
	
	//return mask;
}

inline float GetUnderwaterMask(float waterMask)
{
	return waterMask > 0.5;
}

inline bool GetSurfaceMask(float waterMask)
{
	return abs(waterMask - 0.25) < 0.01;
}


inline float GetWaterHalfLineTensionMask(float2 uv)
{
	float intersectionMask = 0;
	float aquariumMask = 0;
	float2 scaledUV = GetWaterPrePassUV(uv);

	#ifdef KWS_CAMERA_UNDERWATER
		intersectionMask = SAMPLE_TEXTURE_LOD(KWS_WaterIntersectionHalfLineTensionMaskRT, sampler_linear_clamp, scaledUV, 0).x;
		intersectionMask *= 1.4;
		if(intersectionMask >= 0.99) intersectionMask = saturate((1.2 - intersectionMask) * 5);
	#endif
	#ifdef KWS_USE_AQUARIUM_RENDERING
		aquariumMask = SAMPLE_TEXTURE_LOD(KWS_WaterPrePassRT0, sampler_linear_clamp, scaledUV, 0).w;
		intersectionMask = max(intersectionMask, aquariumMask);
	#endif

	return intersectionMask;
}


inline float GetWaterDepth(float2 uv)
{
	return SAMPLE_TEXTURE_LOD(KWS_WaterDepthRT, sampler_point_clamp, GetWaterPrePassUV(uv), 0).x;
}


inline float GetWaterBackfaceDepth(float2 uv)
{
	return SAMPLE_TEXTURE_LOD(KWS_WaterBackfaceDepthRT, sampler_point_clamp, GetWaterBackfacePrePassUV(uv), 0).x;
}

inline uint GetWaterID(float2 uv)
{
	float waterID = SAMPLE_TEXTURE_LOD(KWS_WaterPrePassRT0, sampler_point_clamp, GetWaterPrePassUV(uv), 0).x;
	return waterID * KWS_WATER_MASK_ENCODING_VALUE;
}

//Front depth (x), Back depth (y)
inline float2 GetWaterVolumeDepth(float2 uv,float surfaceZ, float sceneZ, float waterMask)
{
	float2 volumeDepth = 0;
	
	volumeDepth.x = surfaceZ;
	volumeDepth.y = sceneZ;

	#if KWS_USE_AQUARIUM_RENDERING
		volumeDepth.y = max(volumeDepth.y, GetWaterBackfaceDepth(uv));
	#endif

	bool underwaterMask = GetUnderwaterMask(waterMask);
	volumeDepth.y = lerp(volumeDepth.y, max(volumeDepth.x, sceneZ), underwaterMask);
	volumeDepth.x = lerp(volumeDepth.x, 1, underwaterMask);

	if(volumeDepth.x < sceneZ) volumeDepth.x = 0;

	return volumeDepth;
}

//Front depth (x), Back depth (y)
inline float2 GetWaterVolumeDepth(float2 uv, float sceneZ, float waterMask)
{
	float2 volumeDepth = 0;
	
	volumeDepth.x = GetWaterDepth(uv).x;
	volumeDepth.y = sceneZ;

	#if KWS_USE_AQUARIUM_RENDERING
		volumeDepth.y = max(volumeDepth.y, GetWaterBackfaceDepth(uv));
	#endif

	bool underwaterMask = GetUnderwaterMask(waterMask);
	volumeDepth.y = lerp(volumeDepth.y, max(volumeDepth.x, sceneZ), underwaterMask);
	volumeDepth.x = lerp(volumeDepth.x, 1, underwaterMask);

	if(volumeDepth.x < sceneZ) volumeDepth.x = 0;

	return volumeDepth;
}

inline float GetBoxExtrude(float3 pos, float3x3 rotationMatrix, float3 size)
{
	float3 rotatedPos = mul(rotationMatrix, pos).xyz;
	float3 d = abs(rotatedPos) - size;
	return length(max(d, 0)) + vmax(min(d, 0));
}


float3 ComputeExtrudeMask(float3 worldPos, inout float3 surfaceMask)
{
	float3 offset = 0;
	//#ifndef SHADER_API_PS5 //todo check what cause compute buffer binding error?
	#if defined (KWS_USE_EXTRUDE_MASK)

		if(KWS_WaterVolumeDataBufferLength > 0)
		{
			UNITY_LOOP
			for(int dataIdx = 0; dataIdx < KWS_WaterVolumeDataBufferLength; dataIdx++)
			{
				WaterVolumeData data = KWS_WaterVolumeDataBuffer[dataIdx];
				float sdfBox = GetBoxExtrude(worldPos - data.Position, (float3x3)data.Rotation, data.Size);
				if(sdfBox < 0) 
				{
					offset.y += data.ExtrudeHeight;
					surfaceMask.x = saturate( data.ExtrudeHeight + 1);
					if(sdfBox < -2 && data.ExtrudeHeight < 0) surfaceMask.x = data.SaveExtrudedSurfaceWaves;

				}
			}
		}

	#endif
	
	return offset;
}

float3 ComputeExtrudeMask(float3 worldPos)
{
	float3 offset = 0;

	#if defined (KWS_USE_EXTRUDE_MASK)

	if(KWS_WaterVolumeDataBufferLength > 0)
	{
		UNITY_LOOP
		for(int dataIdx = 0; dataIdx < KWS_WaterVolumeDataBufferLength; dataIdx++)
		{
			WaterVolumeData data = KWS_WaterVolumeDataBuffer[dataIdx];
			float sdfBox = GetBoxExtrude(worldPos - data.Position, (float3x3)data.Rotation, data.Size);
			if(sdfBox < 0) 
			{
				offset.y += data.ExtrudeHeight;
			}
		}
	}
	
	#endif

	return offset;
}

/*
DECLARE_TEXTURE(KWS_DebugTexture);

inline float4 GetDebugTexture(float2 uv)
{
	return SAMPLE_TEXTURE_LOD(KWS_DebugTexture, sampler_point_clamp, GetWaterPrePassUV(uv), 0);
}

inline float2 GetWaterVolumePrePassVertex(float3 worldPos)
{
	if(KWS_WaterVolumeDataBufferLength == 0) return 0;
	
	WaterVolumeData data = KWS_WaterVolumeDataBuffer[0];
	float sdf = KWS_SDF_Box(worldPos - data.Position.xyz, (float3x3)data.Rotation, data.Size.xyz); 
	float2 result = float2(sdf, data.InstanceID);
	
	for(uint idx = 1; idx < KWS_WaterVolumeDataBufferLength; idx++)
	{
		data = KWS_WaterVolumeDataBuffer[idx];
		sdf = KWS_SDF_Box(worldPos - data.Position.xyz, (float3x3)data.Rotation, data.Size.xyz); 
		//result.x = min(result.x, sdf);
		if(sdf < result.x) result = float2(sdf, data.InstanceID);
	}

	return result;
}

//inline float GetWaterVolumePrePass(float3 worldPos, out uint id)
//{
//	if(KWS_WaterVolumeDataBufferLength == 0) return 0;
	
//	float result = WATER_VOLUME_PRE_PASS_MAX_VALUE + 1;
//	id = 0;
//	for(uint idx = 0; idx < KWS_WaterVolumeDataBufferLength; idx++)
//	{
//		WaterVolumeData data = KWS_WaterVolumeDataBuffer[idx];
//		float sdf = KWS_SDF_Box(worldPos - data.Position.xyz, (float3x3)data.Rotation, data.Size.xyz); 
//		//result = min(result, sdf);
//		if(sdf < result) 
//		{
//			result = sdf;
//			id = data.InstanceID;
//		}
//	}
	
//	return result;
//}

inline float3 GetWaterVolumePrePass(float3 worldPos, out uint id)
{
	if(KWS_WaterVolumeDataBufferLength == 0) return 0;
	
	//float3 rayDir = GetWorldSpaceViewDirNorm(worldPos);
	float3 startPos = GetCameraAbsolutePosition();
	float3 rayDir =  normalize(worldPos - startPos);
	float distanceToWaterSurface = length(worldPos - startPos);
	float3 result = WATER_VOLUME_PRE_PASS_MAX_VALUE + 1;
	id = 0;
	for(uint idx = 0; idx < KWS_WaterVolumeDataBufferLength; idx++)
	{
		WaterVolumeData data = KWS_WaterVolumeDataBuffer[idx];
		
		float2 box = KWS_IntersectionBox(startPos - data.Position.xyz, rayDir, (float3x3)data.Rotation, data.Size.xyz); 
		float sdf = KWS_SDF_Box(worldPos - data.Position.xyz, (float3x3)data.Rotation, data.Size.xyz); 
		//box.x = max(box.x, 0.0001);

		if(box.x < box.y && box.y > 0.0 && box.x < distanceToWaterSurface)
		{
			if(result.x >= WATER_VOLUME_PRE_PASS_MAX_VALUE || box.x < result.x) id = data.InstanceID;
			//if(sdf < 0 && (box.x < result.x)) id = data.InstanceID;
			
			result.x = min(result.x, box.x);
			result.y = min(result.y, box.y);
			result.z = min(result.z, sdf);
		}
	}

	//if(result.x != 0)
	//{
	//	float4 clipPos = UnityWorldToClipPos(worldPos + result.x * rayDir);
	//	float frontZ = saturate(clipPos.z / clipPos.w);
	//	clipPos = UnityWorldToClipPos(worldPos + result.y * rayDir);
	//	float backZ = saturate(clipPos.z / clipPos.w);
	//	result.xy = float2(frontZ, backZ);
	//}
	return result;
}


inline float GetInsideWaterVolumeMask(float volumeDepthFront)
{
	return saturate((volumeDepthFront - 0.999) * 1000000);
}
*/

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////





//////////////////////////////////////////////    VolumetricLighting_Pass    //////////////////////////////////////////////

DECLARE_TEXTURE(KWS_VolumetricLightRT);
DECLARE_TEXTURE(KWS_VolumetricLightAdditionalDataRT);
float4 KWS_VolumetricLightRT_TexelSize;
float4 KWS_VolumetricLight_RTHandleScale;

DECLARE_TEXTURE(KWS_VolumetricLightRT_Last);
float4 KWS_VolumetricLightRT_Last_TexelSize;
float4 KWS_VolumetricLightRT_Last_RTHandleScale;

struct VolumetricLightAdditionalData
{
	half SurfaceDirShadow;
	half SceneDirShadow;
	half AdditionalLightsAttenuation;
};


inline half3 GetVolumetricLight(float2 uv)
{
	//float2 scaledUV = GetRTHandleUV(uv, KWS_VolumetricLightRT_TexelSize.xy, 1.0, KWS_VolumetricLight_RTHandleScale.xy);
	return SAMPLE_TEXTURE_LOD(KWS_VolumetricLightRT, sampler_linear_clamp, saturate(uv), 0).xyz;
}

//(R) surface dir shadow, (G) scene dir shadow, (B) additional lights attenuation
inline VolumetricLightAdditionalData GetVolumetricLightAdditionalData(float2 uv)
{
	//float2 scaledUV = GetRTHandleUV(uv, KWS_VolumetricLightRT_TexelSize.xy, 1.0, KWS_VolumetricLight_RTHandleScale.xy);
	float3 rawData = SAMPLE_TEXTURE_LOD(KWS_VolumetricLightAdditionalDataRT, sampler_linear_clamp, saturate(uv), 0).xyz;
	VolumetricLightAdditionalData volumeData;
	volumeData.SurfaceDirShadow = rawData.x;
	volumeData.SceneDirShadow = rawData.y;
	volumeData.AdditionalLightsAttenuation = rawData.z;
	return volumeData;
}

inline float GetVolumeLightInDepthTransmitance(float waterHeight, float currentHeight, float transparent, uint waterID)
{
	if(KWS_MeshTypeArray[waterID] == KWS_MESH_TYPE_RIVER) return 1;

	//at far distance currentHeight has some float precission error (water depth -> world pos) and random lines. In this case I can just add small heigh offset relative to water surface <---> camera
	float distanceToCamera = saturate((GetCameraAbsolutePosition().y - waterHeight) * 0.01) * 10;
	float distanceToWaterSurface = waterHeight - currentHeight - distanceToCamera - 1;
	float lightInDepthTransmitance = saturate(exp(-distanceToWaterSurface / max(2, transparent)));
	return lightInDepthTransmitance;
}


inline float GetVolumeLightSunAngleAttenuation(float3 lightDir, float3 normal = float3(0, 1, 0))
{
	return saturate(dot(lightDir, normal));
}

inline half3 GetVolumetricSurfaceLight(float4 volumeScattering, float3 normal, float exposure)
{
	float3 ambient = GetAmbientColor(exposure) * 0.5;
	float3 dirLight = GetVolumeLightSunAngleAttenuation(GetMainLightDir(), normal) * GetMainLightColor(exposure);

	return (ambient + dirLight * volumeScattering.a + volumeScattering.rgb);
}


inline half4 GetVolumetricLightWithAbsorbtion(float2 uv, float2 refractionUV, float transparent, float3 tubidityColor, float3 dyeColor, float3 sceneColor, float2 volumeDepth, uint waterID, float exposure, float3 sceneColorAdditiveAlbedo)
{
	float3 frontPos = GetWorldSpacePositionFromDepth(refractionUV, volumeDepth.x);
	float3 backPos = GetWorldSpacePositionFromDepth(refractionUV, volumeDepth.y);
	float rayLength = length(backPos - frontPos);
	if(volumeDepth.x == 0) rayLength = 0.1;
	
	float dyeOverrideFactor = dot(dyeColor, 0.33);
	float3 absorbtionColor = lerp(1-dyeColor, float3(1.0, 0.13, 0.02), dyeOverrideFactor);
	float absorbtionMultiplier = lerp(1, 0.2, saturate((transparent - KWS_MAX_TRANSPARENT) / KWS_MAX_TRANSPARENT));

	float3 finalAbsorbtion = max(float3(0.0005, 0.001, 0.025), exp2(-rayLength * absorbtionColor * absorbtionMultiplier));

	rayLength = min(rayLength, min(KWS_MAX_TRANSPARENT, transparent));
	float extintionIntegralApproximationCoeff = (transparent * 0.25) * (transparent * 0.25);
	float extinction = 1-saturate(exp2(-rayLength / extintionIntegralApproximationCoeff));

	float waterSurfaceShadow = 1;
	half sceneAttenuation = GetVolumeLightInDepthTransmitance(KWS_WaterPositionArray[waterID].y, backPos.y, transparent, waterID);

	#if KWS_USE_VOLUMETRIC_LIGHT
		float3 scattering = GetVolumetricLight(refractionUV);
		float3 scatteringWithOffset = GetVolumetricLight(uv);
		if(scattering.x + scattering.y + scattering.z == 0) scattering = scatteringWithOffset;

		VolumetricLightAdditionalData volumeLightData = GetVolumetricLightAdditionalData(uv);

		sceneAttenuation = max(sceneAttenuation, volumeLightData.AdditionalLightsAttenuation);
		waterSurfaceShadow = volumeLightData.SurfaceDirShadow;
	#else
		float phaseG = GetVolumeLightSunAngleAttenuation(GetMainLightDir());
		float3 sceneLight = 0.5 * GetAmbientColor(exposure) + GetMainLightColor(exposure) *  phaseG;
		float4 scattering = half4(0.5 * tubidityColor * saturate(sceneLight), 1.0);
	#endif

	sceneColor *= sceneAttenuation;

	#ifdef KWS_SHARED_API_INCLUDED
		float3 albedo = sceneColorAdditiveAlbedo * dot(scattering.xyz, 0.33);
		float3 finalColor = lerp(finalAbsorbtion * sceneColor + clamp(albedo, 0, 2), scattering.xyz, extinction);
	#else
		float3 finalColor = lerp(finalAbsorbtion * sceneColor, scattering.xyz, extinction);
	#endif
	
	return float4(finalColor, waterSurfaceShadow);
}

inline half3 GetVolumetricLightWithAbsorbtionByDistance(float2 uv, float2 refractionUV, float transparent, float3 tubidityColor, float3 dyeColor, float3 sceneColor, float rayLength, uint waterID, float exposure, float3 sceneColorAdditiveAlbedo)
{
	float dyeOverrideFactor = dot(dyeColor, 0.33);
	float3 absorbtionColor = lerp(1-dyeColor, float3(1.0, 0.13, 0.02), dyeOverrideFactor);
  	float absorbtionMultiplier = lerp(1, 0.2, saturate((transparent - KWS_MAX_TRANSPARENT) / KWS_MAX_TRANSPARENT));

	float3 finalAbsorbtion = max(float3(0.0005, 0.001, 0.025), exp2(-rayLength * absorbtionColor * absorbtionMultiplier));

	rayLength = min(rayLength, min(KWS_MAX_TRANSPARENT, transparent));
	float extintionIntegralApproximationCoeff = (transparent * 0.25) * (transparent * 0.25);
	float extinction = 1-saturate(exp2(-rayLength / extintionIntegralApproximationCoeff));


	#if KWS_USE_VOLUMETRIC_LIGHT
		float3 scattering = GetVolumetricLight(refractionUV);
		float3 scatteringWithOffset = GetVolumetricLight(uv);
		if(scattering.x + scattering.y + scattering.z == 0) scattering = scatteringWithOffset;
	#else
		float phaseG = GetVolumeLightSunAngleAttenuation(GetMainLightDir());
		float3 sceneLight = 0.5 * GetAmbientColor(exposure) + GetMainLightColor(exposure) *  phaseG;
		float4 scattering = half4(0.5 * tubidityColor * saturate(sceneLight), 1.0);
	#endif

	#ifdef KWS_SHARED_API_INCLUDED
		float3 albedo = sceneColorAdditiveAlbedo * dot(scattering.xyz, 0.33);
		float3 finalColor = lerp(finalAbsorbtion * sceneColor + clamp(albedo, 0, 2), scattering.xyz, extinction);
	#else
		float3 finalColor = lerp(finalAbsorbtion * sceneColor, scattering.xyz, extinction);
	#endif
	
	return finalColor;
}


inline half4 GetVolumetricLightLastFrame(float2 uv)
{
	float2 scaledUV = GetRTHandleUV(uv, KWS_VolumetricLightRT_TexelSize.xy, 1.0, KWS_VolumetricLight_RTHandleScale.xy);
	//scaledUV += KWS_VolumetricLightRT_TexelSize.xy * 0.5;
	return SAMPLE_TEXTURE_LOD(KWS_VolumetricLightRT_Last, sampler_point_clamp, uv, 0);
}


/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


//////////////////////////////////////////////    ScreenSpaceReflection_Pass    //////////////////////////////////////////////
DECLARE_TEXTURE(KWS_ScreenSpaceReflectionRT);
float4 KWS_ScreenSpaceReflectionRT_TexelSize;
float4 KWS_ScreenSpaceReflection_RTHandleScale;
#define KWS_ScreenSpaceReflectionMaxMip 4

inline float2 GetScreenSpaceReflectionNormalizedUV(float2 uv)
{
	uv = GetRTHandleUV(uv, KWS_ScreenSpaceReflectionRT_TexelSize.xy, 0.5, KWS_ScreenSpaceReflection_RTHandleScale.xy);
	//uv -= KWS_ScreenSpaceReflectionRT_TexelSize.xy * 0.5;
	return uv;
	//return clamp(uv, 0.001, 0.999) * KWS_ScreenSpaceReflection_RTHandleScale.xy;

}

inline half4 GetScreenSpaceReflection(float2 uv, float3 worldPos)
{
	float2 ssrUV = GetScreenSpaceReflectionNormalizedUV(uv);
	float mipLevel = CalcMipLevel(ssrUV * KWS_ScreenSpaceReflectionRT_TexelSize.zw);

	float distance = length(worldPos.xz - GetCameraAbsolutePosition().xz);
	float anisoScaleRelativeToDistance = saturate(distance * 0.05);

	float lod = min(mipLevel, KWS_ScreenSpaceReflectionMaxMip);
	//lod = anisoScaleRelativeToDistance * 2;

	float4 res = SAMPLE_TEXTURE_LOD(KWS_ScreenSpaceReflectionRT, sampler_trilinear_clamp, ssrUV, lod);
	//res.a = saturate(res.a); //I use negative alpha to minimize edge bilinear interpolation artifacts.
	return res;
}

float KWS_ScreenSpaceBordersStretching;
inline half4 GetScreenSpaceReflectionWithStretchingMask(float2 uv, float3 worldPos)
{
	#if defined(STEREO_INSTANCING_ON)
		uv -= mul((float2x2)UNITY_MATRIX_V, float2(0, KWS_ReflectionClipOffset)).xy;
	#else
		uv.y -= KWS_ReflectionClipOffset;
	#endif

	float AngleStretch = max(0.25, saturate(-KWS_CameraForward.y * 1.5));
	float ScreenStretch = saturate(abs(uv.x * 2 - 1) - 0.8);
	float uvOffset = -AngleStretch * ScreenStretch * KWS_ScreenSpaceBordersStretching * 10;
	uv.x = uv.x * (1 + uvOffset * 2) - uvOffset;
	//float stretchingMask = 1 - abs(refl_uv.x * 2 - 1);
	//refl_uv.x = lerp(refl_uv.x * (1 - Test4.x * 2) + Test4.x, refl_uv.x, AngleStretch * ScreenStretch);
	return GetScreenSpaceReflection(uv, worldPos);
}



////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


//////////////////////////////////////////////    Reflection_Pass    //////////////////////////////////////////////////////////
DECLARE_TEXTURE(KWS_PlanarReflectionRT);
int KWS_PlanarReflectionInstanceID;
float4 KWS_PlanarReflectionRT_TexelSize;
TextureCube KWS_CubemapReflectionRT;

#define KWS_PlanarReflectionMaxMip 4

inline half3 GetPlanarReflection(float2 refl_uv)
{
	float mipLevel = CalcMipLevel(refl_uv * KWS_PlanarReflectionRT_TexelSize.zw);
	float lod = min(mipLevel, KWS_PlanarReflectionMaxMip);
	return SAMPLE_TEXTURE_LOD(KWS_PlanarReflectionRT, sampler_trilinear_clamp, refl_uv, lod).xyz;
}

inline half3 GetPlanarReflectionRaw(float2 refl_uv)
{
	return SAMPLE_TEXTURE_LOD(KWS_PlanarReflectionRT, sampler_trilinear_clamp, refl_uv, 0).xyz;
}

inline half3 GetPlanarReflectionWithClipOffset(float2 refl_uv)
{
	#if defined(STEREO_INSTANCING_ON)
		refl_uv -= mul((float2x2)UNITY_MATRIX_V, float2(0, KWS_ReflectionClipOffset)).xy;
	#else
		refl_uv.y -= KWS_ReflectionClipOffset;
	#endif
	return GetPlanarReflection(refl_uv);
}

///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



//////////////////////////////////////////////    Caustic_Pass    //////////////////////////////////////////////////////////
Texture2DArray KWS_CausticRTArray;
float4 KWS_CausticRTArray_TexelSize;
float KWS_CaustisDispersionStrength;

float KWS_InstanceToCausticID[MAX_WATER_INSTANCES];
//float KWS_InstanceToCausticViewportScale[MAX_WATER_INSTANCES];

//inline half GetCaustic(float2 uv, uint waterID)
//{
//	float causticSlice = KWS_InstanceToCausticID[waterID];
//	float causticScale = KWS_InstanceToCausticViewportScale[waterID];
//	float2 causticUV = frac(uv) * causticScale;
//	float2 causticDerivativeUV = uv * causticScale * 0.5;
//	return KWS_CausticRTArray.SampleGrad(sampler_linear_repeat, float3(causticUV, causticSlice), ddx(causticDerivativeUV), ddy(causticDerivativeUV));
//}

inline float3 GetCaustic(float2 uv, uint waterID)
{
	float causticSlice = KWS_InstanceToCausticID[waterID];
	
	float3 caustic = 1;
	#ifdef USE_DISPERSION
		float2 offset = KWS_CausticRTArray_TexelSize.x * KWS_CaustisDispersionStrength;
		caustic.x = KWS_CausticRTArray.Sample(sampler_linear_repeat, float3(uv - offset, causticSlice)).x;
		caustic.y = KWS_CausticRTArray.Sample(sampler_linear_repeat, float3(uv, causticSlice)).x;
		caustic.z = KWS_CausticRTArray.Sample(sampler_linear_repeat, float3(uv + offset, causticSlice)).x;
	#else
		caustic = KWS_CausticRTArray.Sample(sampler_linear_repeat, float3(uv, causticSlice)).x;
	#endif

	return caustic;
}

inline half GetCausticLod(float2 uv, uint waterID, float lod)
{
	float causticSlice = KWS_InstanceToCausticID[waterID];
	return KWS_CausticRTArray.SampleLevel(sampler_linear_repeat, float3(uv, causticSlice), lod).x;
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



//////////////////////////////////////////////    Flowmap_Pass    /////////////////////////////////////////////////////////////////////////////////////////
Texture2D KW_FlowMapTex;

#define FLOW_MAP_STRENGTH 10
#define FLOW_MAP_THRESHOLD 0.1
#define FLOW_MAP_FLOW_FIX 1.25

struct FlowMapData
{
	bool isFlowArea;
	float2 uvOffset1;
	float2 uvOffset2;
	float flowLerpFix;
	float flowLerp;
};

inline float2 GetFlowmap(float2 uv)
{
	float2 flowMapUV = (uv - KW_FlowMapOffset.xz) / KW_FlowMapSize + 0.5;
	return KW_FlowMapTex.SampleLevel(sampler_linear_clamp, flowMapUV, 0).xy * 2 - 1;
}

inline FlowMapData GetFlowmapData(float2 uv)
{
	float2 flowMap = GetFlowmap(uv) * FLOW_MAP_STRENGTH;

	float time = KWS_ScaledTime * KW_FlowMapSpeed * 0.5;
	//time = Test4.w;
	half time1 = frac(time + 0.5);
	half time2 = frac(time);
	
	FlowMapData data;
	data.isFlowArea = (abs(flowMap.x) + abs(flowMap.y)) > FLOW_MAP_THRESHOLD;
	data.uvOffset1 = -flowMap * time1 * data.isFlowArea;
	data.uvOffset2 = -flowMap * time2;
	data.flowLerp = abs((0.5 - time1) / 0.5);
	data.flowLerpFix = lerp(FLOW_MAP_FLOW_FIX, 1, abs(data.flowLerp * 2 - 1));

	return data;
}

inline float3 GetFftWavesDisplacementWithFlowMap(float3 worldPos)
{
	FlowMapData data = GetFlowmapData(worldPos.xz);
	float3 result = GetFftWavesDisplacement(worldPos + float3(data.uvOffset1.x, 0, data.uvOffset1.y));

	UNITY_BRANCH
	if (data.isFlowArea)
	{
		float3 result2 = GetFftWavesDisplacement(worldPos + float3(data.uvOffset2.x, 0, data.uvOffset2.y));
		result = lerp(result, result2, data.flowLerp);
		//result.xz *= data.flowLerpFix;
		return result;
	}
	else return result;
}


inline float3 GetFftWavesNormalFoamWithFlowmap(float3 worldPos)
{
	FlowMapData data = GetFlowmapData(worldPos.xz);
	float3 result = GetFftWavesNormalFoam(worldPos + float3(data.uvOffset1.x, 0, data.uvOffset1.y));

	UNITY_BRANCH
	if (data.isFlowArea)
	{
		float3 result2 = GetFftWavesNormalFoam(worldPos + float3(data.uvOffset2.x, 0, data.uvOffset2.y));
		result = lerp(result, result2, data.flowLerp);
		result.xz *= data.flowLerpFix;
		return result;
	}
	else return result;
}

inline float3 GetCausticWithFlowmap(float2 causticUV, float3 worldPos, uint waterID, float domainSize)
{
	FlowMapData data = GetFlowmapData(worldPos.xz);
	float3 caustic = GetCaustic(causticUV + data.uvOffset1 / domainSize, waterID);

	UNITY_BRANCH
	if (data.isFlowArea)
	{
		float3 caustic2 = GetCaustic(causticUV + data.uvOffset2 / domainSize, waterID);
		caustic = lerp(caustic, caustic2, data.flowLerp);
		caustic *= data.flowLerpFix;
		return caustic;
	}
	else return caustic;
}

inline float4 SampleTextureWithFlowmap(float3 worldPos, Texture2D tex, SamplerState state, float2 uvScale)
{
	FlowMapData data = GetFlowmapData(worldPos.xz);
	float4 result = tex.Sample(state, (worldPos.xz + data.uvOffset1) * uvScale);

	UNITY_BRANCH
	if (data.isFlowArea)
	{
		float4 result2 = tex.Sample(state, (worldPos.xz + data.uvOffset2) * uvScale);
		result = lerp(result, result2, data.flowLerp);
		return result;
	}
	else
		return result;
}

///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



//////////////////////////////////////////////    Otrho depth    //////////////////////////////////////////////////////////
Texture2D KWS_WaterOrthoDepthRT;
float3 KWS_OrthoDepthPos;
float3 KWS_OrthoDepthNearFarSize;

float2 GetWaterOrthoDepthUV(float3 worldPos)
{
	return (worldPos.xz - KWS_OrthoDepthPos.xz) / KWS_OrthoDepthNearFarSize.z + 0.5;
}

float GetWaterOrthoDepth(float2 uv)
{
	float near = KWS_OrthoDepthNearFarSize.x;
	float far = KWS_OrthoDepthNearFarSize.y;
	float terrainDepth = KWS_WaterOrthoDepthRT.SampleLevel(sampler_linear_clamp, uv, 0).r * (far - near) - far;
	return terrainDepth;
}

float GetWaterOrthoDepth(float3 worldPos)
{
	float2 uv = GetWaterOrthoDepthUV(worldPos);
	return GetWaterOrthoDepth(uv);
}
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



//////////////////////////////////////////////    shoreline    //////////////////////////////////////////////////////////
Texture2D KWS_ShorelineWavesDisplacement;
Texture2D KWS_ShorelineWavesNormal;

float4 KWS_ShorelineAreaPosSize;
float KWS_ShorelineAreaWavesCount;

struct ShorelineDataStruct
{
	float4x4 worldMatrix;

	int waveID;
	float3 position;

	float angle;
	float3 size;

	float timeOffset;
	float3 scale;

	int flip;
	float3 pad;
};
StructuredBuffer<ShorelineDataStruct> KWS_ShorelineDataBuffer;

inline float2 GetShorelineWavesUV(float3 worldPos)
{
	return (worldPos.xz - KWS_ShorelineAreaPosSize.xz) / KWS_ShorelineAreaPosSize.w + 0.5;
}

inline half GetShorelineWavesMask(float2 uv, float distanceToCamera)
{
	uv = 1 - saturate(abs(uv * 2 - 1));
	return saturate((uv.x * uv.y - 0.01) * 5);
}

inline float3 GetShorelineDisplacement(float3 worldPos)
{
	if (KWS_ShorelineAreaWavesCount == 0) return 0;

	float2 uv = GetShorelineWavesUV(worldPos);
	if (IsOutsideUV(uv)) return 0;
	return KWS_ShorelineWavesDisplacement.SampleLevel(sampler_linear_clamp, uv, 0).xyz;
}


inline float3 ComputeShorelineOffset(float3 worldPos, float3 waterOffset, float multiplier = 1.0)
{
	if (KWS_ShorelineAreaWavesCount == 0) return waterOffset;

	float2 uv = GetShorelineWavesUV(worldPos);
	if (IsOutsideUV(uv)) return waterOffset;
	float3 beachOffset = KWS_ShorelineWavesDisplacement.SampleLevel(sampler_linear_clamp, uv, 0).xyz;

	float2 orthoDepthUV = GetWaterOrthoDepthUV(worldPos);
	float terrainDepth = GetWaterOrthoDepth(orthoDepthUV);
	if (!IsOutsideUV(orthoDepthUV)) waterOffset = lerp(waterOffset, 0, saturate(terrainDepth + 0.85));

	return waterOffset +beachOffset * multiplier;
}

inline float3 ComputeShorelineNormal(half3 normal, float3 worldPos)
{
	if (KWS_ShorelineAreaWavesCount == 0) return normal;

	float2 uv = GetShorelineWavesUV(worldPos);
	if (IsOutsideUV(uv)) return normal;
	
	float2 waveNormalsRaw = KWS_ShorelineWavesNormal.SampleLevel(sampler_linear_clamp, uv, 0).xy;
	float3 waveNormals = normalize(float3(waveNormalsRaw.x, 1, waveNormalsRaw.y));

	normal = KWS_BlendNormals(normal, waveNormals);
	return normal;
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////






//////////////////////////////////////////////    Dynamic waves    //////////////////////////////////////////////////////////

Texture2D DynamicWavesMaskRT;
float4 DynamicWavesMaskPos;

Texture2D KWS_DynamicWaves;
Texture2D KWS_DynamicWavesNormal;

float KW_DynamicWavesAreaSize;

#define DYNAMIC_WAVE_PROCEDURAL_MASK_TYPE_SPHERE 1

struct KWS_DynamicWavesMask
{
	uint proceduralType;
	float force;
	float waterHeight;
	float size;

	float3 position;
	float _pad;
};
StructuredBuffer<KWS_DynamicWavesMask> KWS_DynamicWavesMaskBuffer;

inline float2 GetDynamicWavesMaskUV(float3 worldPos)
{
	return (worldPos.xz - DynamicWavesMaskPos.xz) / DynamicWavesMaskPos.w + 0.5;
}

inline float GetDynamicWavesMask(float3 worldPos)
{
	float2 uv = GetDynamicWavesMaskUV(worldPos);
	
	UNITY_BRANCH
	if (IsOutsideUV(uv)) return 0;
	else
	{
		return DynamicWavesMaskRT.SampleLevel(sampler_linear_clamp, uv, 0).x;
	}
}

inline float GetDynamicWavesDisplacement(float3 worldPos)
{
	float2 uv = GetDynamicWavesMaskUV(worldPos);
	
	UNITY_BRANCH
	if (IsOutsideUV(uv)) return 0;
	else
	{
		return KWS_DynamicWaves.SampleLevel(sampler_linear_clamp, uv, 0).x;
	}
}


inline float3 GetDynamicWavesNormals(float3 worldPos)
{
	float2 uv = GetDynamicWavesMaskUV(worldPos);
	
	UNITY_BRANCH
	if (IsOutsideUV(uv)) return float3(0, 1, 0);
	else
	{
		float3 dynamicWavesNormals = KWS_DynamicWavesNormal.SampleLevel(sampler_linear_clamp, uv, 0).xyz * 2 - 1;
		return normalize(float3(dynamicWavesNormals.x * 0.35, 1, -dynamicWavesNormals.y * 0.35));
	}
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////




//////////////////////////////////////////////   Underwater    //////////////////////////////////////////////////////////
#define UNDERWATER_DEPTH_SCALE 0.75 

 


/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#endif