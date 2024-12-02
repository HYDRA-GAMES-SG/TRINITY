#define MaxUint 4294967295u
#define MaxHalf 65504.0
#define STRETCH_THRESHOLD 0.75
#define MAX_HEIGHT_STRETCH_METERS 100

float4 _RTSize;
float KWS_AverageInstancesHeight;
uint _DepthHolesFillDistance;
uint KWS_ReprojectedFrameReady;
uint UseScreenSpaceReflectionSky;
//float KWS_ScreenSpaceBordersStretching;

float KWS_ActiveSsrInstances[MAX_WATER_INSTANCES];

#if defined(STEREO_INSTANCING_ON)
	RWTexture2DArray<float4> ColorRT;
	RWTexture2DArray<float4> KWS_LastTargetRT;
	#define GetTextureID(id) uint3(id, unity_StereoEyeIndex)
	#define GetBufferOffset(stereoIdx)  _RTSize.x * _RTSize.y * stereoIdx
	#define SetStereoIndex(x) unity_StereoEyeIndex = x
#else
	RWTexture2D<float4> ColorRT;
	RWTexture2D<float4> KWS_LastTargetRT;
	#define GetTextureID(id) id
	#define GetBufferOffset(stereoIdx) 0
	#define SetStereoIndex(x)
#endif
RWStructuredBuffer<uint> HashRT;

StructuredBuffer<float> KWS_WaterSurfaceHeights;
#define DiscardIfOutOfBorder(id) uint2 rtSize = _RTSize.xy; if (id.x > rtSize.x || id.y > rtSize.y) return;

#if defined(STEREO_INSTANCING_ON)

	#define STEREO_LOOP for (int stereoIdx = 0; stereoIdx <= 1; stereoIdx++) { SetStereoIndex(stereoIdx);
	#define END_STEREO_LOOP }
	#define CONTINUTE_STEREO_LOOP continue;
#else
	#define STEREO_LOOP
	#define END_STEREO_LOOP
	#define CONTINUTE_STEREO_LOOP return;
#endif

half ComputeUVFade(float2 screenUV, float underwaterMask = 0)
{
	UNITY_BRANCH
	if (screenUV.x <= 0.001 || screenUV.x > 0.999 || screenUV.y < 0.001 || screenUV.y > 0.999) return 0;
	else
	{
		float fringeY = lerp(1 - screenUV.y, screenUV.y, underwaterMask);
		float fringeX = fringeY * (1 - abs(screenUV.x * 2 - 1)) * 300;
		fringeY = fringeY * 10;
		return saturate(fringeY) * saturate(fringeX);
	}
}

///////////////////////////////////////////////////////////////////////////////// kernels ////////////////////////////////////////////////////////////////////////////////////////


[numthreads(8, 8, 1)]
void Clear_kernel(uint3 id : SV_DispatchThreadID)
{
	DiscardIfOutOfBorder(id.xy);
	SetStereoIndex(id.z);
	uint hashOffset = GetBufferOffset(id.z);
	float2 screenUV = id.xy * _RTSize.zw + _RTSize.zw * 0.5;

	#ifdef USE_UNDERWATER_REFLECTION
		//float depth = GetSceneDepth(screenUV);
		//float underwaterMask = GetInsideWaterVolumeMask(GetWaterVolumeDepth(screenUV, depth).x);
		uint waterID = GetWaterID(screenUV);
		//uint waterID = KWS_ActiveSsrInstances[id.z];
		float cameraHeight = GetCameraAbsolutePosition().y;
		float waterHeight = KWS_WaterPositionArray[waterID].y;
		HashRT[id.y * _RTSize.x + id.x + hashOffset] = cameraHeight < waterHeight ? 0 : MaxUint;
	#else
		HashRT[id.y * _RTSize.x + id.x + hashOffset] = MaxUint;
	#endif
}



[numthreads(8, 8, 1)]
void RenderHash_kernel(uint3 id : SV_DispatchThreadID)
{
	DiscardIfOutOfBorder(id.xy);
	float2 screenUV = id.xy * _RTSize.zw + _RTSize.zw * 0.5;
	
	uint waterID = KWS_ActiveSsrInstances[id.z];
	float waterHeight = KWS_WaterPositionArray[waterID].y;
	
	STEREO_LOOP
	float depth = GetSceneDepth(screenUV);
	float3 posWS = GetWorldSpacePositionFromDepth(screenUV, depth);
	
	#ifdef USE_UNDERWATER_REFLECTION
		float underwaterMask = GetCameraAbsolutePosition().y < waterHeight;
		float underwaterHeight = -waterHeight * (underwaterMask * 2 - 1); //todo why unity cant update waterHeight, is it const relative to structured buffer?
		if (GetCameraAbsolutePosition().y < waterHeight)
		{
			if (posWS.y >= waterHeight) CONTINUTE_STEREO_LOOP
		}
		else	
		{
			if (posWS.y <= waterHeight) CONTINUTE_STEREO_LOOP
		}
	#else
		if (posWS.y <= waterHeight) CONTINUTE_STEREO_LOOP
	#endif

	
	float3 reflectPosWS = posWS;
	
	reflectPosWS.y = -reflectPosWS.y + 2 * waterHeight;
	float2 reflectUV = WorldPosToScreenPos(reflectPosWS).xy;

	float HeightStretch = min(posWS.y - waterHeight, MAX_HEIGHT_STRETCH_METERS);
	float AngleStretch = saturate(-KWS_CameraForward.y);
	float ScreenStretch = saturate(abs(reflectUV.x * 2 - 1) - STRETCH_THRESHOLD);
	float uvOffset = HeightStretch * AngleStretch * ScreenStretch * KWS_ScreenSpaceBordersStretching;
	reflectUV.x = reflectUV.x * (1 + uvOffset * 2) - uvOffset;

	if (reflectUV.x < 0.005 || reflectUV.x > 0.995 || reflectUV.y < 0.005 || reflectUV.y > 0.995) CONTINUTE_STEREO_LOOP
	
	uint reprojectedWaterID = GetWaterID(reflectUV);
	if (reprojectedWaterID == 0 || reprojectedWaterID != waterID) CONTINUTE_STEREO_LOOP

	uint2 reflectedScreenID = reflectUV * _RTSize.xy;
	uint hash = id.y << 16 | id.x;
	if (UseScreenSpaceReflectionSky == 0 && depth < 0.0000001) hash = MaxUint - 1;
	uint hashOffset = GetBufferOffset(stereoIdx);

	#ifdef USE_UNDERWATER_REFLECTION
		UNITY_BRANCH
		if (underwaterMask > 0.5) InterlockedMax(HashRT[reflectedScreenID.y * _RTSize.x + reflectedScreenID.x + hashOffset], hash);
		else InterlockedMin(HashRT[reflectedScreenID.y * _RTSize.x + reflectedScreenID.x + hashOffset], hash);
	#else
		InterlockedMin(HashRT[reflectedScreenID.y * _RTSize.x + reflectedScreenID.x + hashOffset], hash);
	#endif
END_STEREO_LOOP
}

[numthreads(8, 8, 1)]
void RenderColorFromHash_kernel(uint3 id : SV_DispatchThreadID)
{
	DiscardIfOutOfBorder(id.xy);
	SetStereoIndex(id.z);

	float2 screenUV = id.xy * _RTSize.zw + _RTSize.zw * 0.5;
	uint hashOffset = GetBufferOffset(id.z);
	uint hashIdx = id.y * _RTSize.x + id.x;
	
	uint left = HashRT[hashIdx + 1 + hashOffset].x;
	uint right = HashRT[hashIdx - 1 + hashOffset].x;
	uint down = HashRT[(id.y + 1) * _RTSize.x + id.x + hashOffset].x;
	uint up = HashRT[(id.y - 1) * _RTSize.x + id.x + hashOffset].x;

	uint hashData;
	
	float waterHeight = 0;
	//#if defined(USE_UNDERWATER_REFLECTION) || defined(USE_UNDERWATER_REFLECTION)
		uint waterID = GetWaterID(screenUV);
		waterHeight = KWS_WaterPositionArray[waterID].y;
	//#endif

	float4 earlyOutColor = 0;
	float exposure = GetExposure();
	float3 worldPos = GetWorldSpacePositionFromDepth(screenUV, 0.0);
	float3 viewDir = normalize(_WorldSpaceCameraPos.xyz - worldPos) * float3(-1, 1, -1);
	

	#ifdef USE_UNDERWATER_REFLECTION
		float underwaterMask = GetCameraAbsolutePosition().y < waterHeight;
		UNITY_BRANCH
		if (underwaterMask > 0.5) hashData = max(left, max(right, max(up, down)));
		else 
		{
			hashData = min(left, min(right, min(up, down)));
			viewDir.y *= worldPos.y >= waterHeight? -1 : 1;
		}
	#else
		hashData = min(left, min(right, min(up, down)));
		viewDir.y *= worldPos.y >= waterHeight? -1 : 1;
	#endif




	float3 fallBackColor = KWS_GetSkyColor(viewDir, KWS_SkyLodRelativeToWind, exposure);
	
	#if KWS_USE_PLANAR_REFLECTION
		//if(GetWaterID(screenUV) == (uint)KWS_PlanarReflectionInstanceID) //todo unity dont update KWS_PlanarReflectionInstanceID, wtf?
		fallBackColor = GetPlanarReflectionRaw(screenUV) * exposure;
	#endif

	if (hashData == MaxUint)
	{
		#if defined(USE_HOLES_FILLING) 
			if (KWS_ReprojectedFrameReady == 1)
			{
				float2 reprojectedUV = WorldPosToScreenPosReprojectedPrevFrame(worldPos, 0).xy;
				float4 lastColor = KWS_LastTargetRT[GetTextureID(reprojectedUV * _RTSize.xy)].xyzw;
				float3 reflectPosWS = worldPos;
				reflectPosWS.y = -reflectPosWS.y + 2 * waterHeight;
				float2 reflectUV = WorldPosToScreenPos(reflectPosWS).xy;
				
				if (IsInsideUV(reflectUV) && dot(lastColor.xyz, 1) > 0.001) earlyOutColor = lastColor;
			}
		#endif

		earlyOutColor.xyz = lerp(fallBackColor, earlyOutColor.xyz, earlyOutColor.w);
		

		ColorRT[GetTextureID(id.xy)] = earlyOutColor;
		return;
	}

	uint2 sampleID = uint2(hashData & 0xFFFF, hashData >> 16);
	float2 sampleUV = (sampleID.xy) * _RTSize.zw + _RTSize.zw * 0.5;

	#ifdef USE_UNDERWATER_REFLECTION
		float fade = ComputeUVFade(sampleUV, underwaterMask);
	#else
		float fade = ComputeUVFade(sampleUV);
	#endif
	
	half3 sampledColor = GetSceneColorPoint(sampleUV);
	half4 finalColor = half4(sampledColor, fade);
	finalColor.xyz = lerp(fallBackColor, finalColor.xyz, fade);

	ColorRT[GetTextureID(id.xy)] = finalColor;
}