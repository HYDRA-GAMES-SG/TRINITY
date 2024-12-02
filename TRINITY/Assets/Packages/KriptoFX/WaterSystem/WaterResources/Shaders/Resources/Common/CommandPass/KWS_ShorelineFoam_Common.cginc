#define EncodedAlphaRange 15.0
#define BufferRange 10000.0
#define EncodeAORange 255.0
#define TERRAIN_DEPTH_OFFSET 0.035

#if defined(KWS_FOAM_USE_FAST_MODE)
	#if defined(STEREO_INSTANCING_ON)
		RWTexture2DArray<uint> _FoamRT;
	#else
		RWTexture2D<uint> _FoamRT;
	#endif
	#define MAX_IDX_RANGE_PIXELS 3
#else
	#if defined(STEREO_INSTANCING_ON)
		RWTexture2DArray<half4> _FoamRT;
	#else
		RWTexture2D<half4> _FoamRT;
	#endif
	#define MAX_IDX_RANGE_PIXELS 4
#endif


RWStructuredBuffer<uint> _FoamBuffer;
RWStructuredBuffer<uint> _FoamLightBuffer;
StructuredBuffer<uint2> _foamDataBuffer;
StructuredBuffer<uint4> _foamDataCountOffsetBuffer;
StructuredBuffer<int> _lodIndexes;

float4 _FoamRTSize;
uint _lodOffset;
uint _StereoIndex;
float _lodSizeMultiplier;

float4x4 _MATRIX_VP;
float4x4 KWS_FoamLocalMatrices;

float4 ComputeTest;
float _MaxParticles;
float _DispatchSize;

//#define BAKE_AO
//#define DEBUG_HEIGHT_OFFSET
float KWS_DebugVatHeightOffset[256];
float KWS_DebugVatFadeoutOffset[256];
RWStructuredBuffer<float> _AOBuffer;
float _customTime;
Texture2D _AfterWaterPassColor;

#if defined(STEREO_INSTANCING_ON)
	#define GetTextureID(id) uint3(id, unity_StereoEyeIndex)
	#define GetBufferOffset  _FoamRTSize.x * _FoamRTSize.y * unity_StereoEyeIndex
	#define GetLodIdx(id) _lodOffset +id.z + _lodCount
	#define SetStereoIndex(x) unity_StereoEyeIndex = x
#else
	#define GetTextureID(id) id
	#define GetBufferOffset 0
	#define SetStereoIndex(x)
#endif

float HashIdx(uint s)
{
	s = s ^ 2747636419u;
	s = s * 2654435769u;
	s = s ^(s >> 16);
	s = s * 2654435769u;
	s = s ^(s >> 16);
	s = s * 2654435769u;
	return float(s) / 4294967295.0f;
}

float3 Unpack_R12G8B12f(uint data)
{
	float3 result;
	result.z = (data & 0xFFF) / 4095.0; //r12 bits = 2^12 - 1 = 4095
	result.y = ((data >> 12) & 0xFF) / 255.0; //g8 bits = 2^8 - 1 = 255
	result.x = ((data >> 20) & 0xFFF) / 4095.0;
	return result;
}


void Unpack_R20G6B6u(uint data, out uint a, out uint b, out uint c)
{
	c = data & 0x3F; //alpha 6 bits
	b = (data >> 6) & 0x3F;  //ao 6 bits
	a = (data >> 12) & 0xFFFFF; //index 20 bits
}

void Unpack_R20G8B4u(uint data, out uint a, out uint b, out uint c)
{
	c = data & 0xF; //alpha 4 bits
	b = (data >> 4) & 0xFF;  //ao 8 bits
	a = (data >> 12) & 0xFFFFF; //index 20 bits
}

uint Pack_R8G8B8A8n(float4 data)
{
	uint4 newData = data * 255.0;
	
	return ((newData.x & 0xFF) << 24)
	| ((newData.y & 0xFF) << 16)
	| ((newData.z & 0xFF) << 8)
	| (newData.w & 0xFF);
}

//-----------------------------------------------------------------------------

struct ParticleData
{
	float3 pos;
	float alpha;
	float ao;
	uint maxParticles;
};

ParticleData GetParticlePosAlpha(ShorelineDataStruct waveData, uint idx, float waveTime)
{
	ParticleData data = (ParticleData)0;
	data.ao = 1;
	data.alpha = -1;
	uint4 countOffset = _foamDataCountOffsetBuffer[waveTime]; //x = count, y = offset, z = next count, w = next offset

	UNITY_BRANCH
	if (idx <= countOffset.x && idx <= countOffset.z) 
	{
		data.maxParticles = max(countOffset.x, countOffset.z);
		uint2 data1 = _foamDataBuffer[idx + countOffset.y];
	
		uint alpha1, nextFrameIdx1, ao1;
		Unpack_R20G8B4u(data1.y, nextFrameIdx1, ao1, alpha1);
		uint2 data2 = _foamDataBuffer[nextFrameIdx1 + countOffset.w];
	
		UNITY_BRANCH
		if (nextFrameIdx1 < idx) 
		{
			float3 pos1 = Unpack_R12G8B12f(data1.x);
			float3 pos2 = Unpack_R12G8B12f(data2.x);

			uint alpha2, nextFrameIdx2, ao2;
			Unpack_R20G8B4u(data2.y, nextFrameIdx2, ao2, alpha2);

			data.pos = lerp(pos1, pos2, frac(waveTime));
			data.alpha = lerp(alpha1, alpha2, frac(waveTime)) / EncodedAlphaRange;
			data.ao = lerp(ao1, ao2, frac(waveTime)) / EncodeAORange;
	
			#if defined(DEBUG_HEIGHT_OFFSET)
				data.pos.y += KWS_DebugVatHeightOffset[(uint) (data.pos.x * 255)];
			#endif
	
			data.pos.xyz -= _Offset.xyz;
			data.pos.xyz *= _Scale;
		}
	}

	
	return data;
}

inline bool IntersectRaySphere(float3 startRay, float3 rayDir, float3 sphereCenter, float radius)
{
	float3 o2c = sphereCenter - startRay;
	float rayDot = dot(rayDir, o2c);

	if (rayDot >= 0.0001f)
	{
		float3 proj = startRay + rayDot * rayDir;
		float3 c2p = proj - sphereCenter;

		float3 nearest = sphereCenter + radius * normalize(c2p);
		float3 p2n = nearest - proj;

		float contact = dot(p2n, c2p);
		if (contact >= 0.0001f) return true;
	}

	return false;
}

/*
void BakeShadowAO(uint waveIdx, uint idx, float time, float exposure)
{
	ShorelineDataStruct waveData = KWS_ShorelineDataBuffer[waveIdx];
	float waveTime = _customTime;

	float ao;
	uint maxParticles;
	float4 posAlpha = GetParticlePosAlpha(waveData, idx, waveTime, ao, maxParticles);
	if (posAlpha.a < 0.0) return;

	float resultAO = 1.0f;
	uint temp;

	for (uint p = 0; p < maxParticles; p += 5)
	{
		float4 nextPos = GetParticlePosAlpha(waveData, p, waveTime, ao, temp);
		if (posAlpha.a < 0.0) continue;
		if (IntersectRaySphere(posAlpha.xyz, _WorldSpaceLightPos0, nextPos, 0.05)) resultAO *= 0.95;
	}
	uint4 countOffset = _foamDataCountOffsetBuffer[waveTime];
	_AOBuffer[idx + countOffset.y] = resultAO;

	uint encodedLight = Pack_R11G11B10f(resultAO);

	float4 worldPos = mul(waveData.worldMatrix, float4(posAlpha.xyz, 1.0));

	float4 screenPos = WorldPosToScreenPos(worldPos);
	float2 screenUV = screenPos.xy;
	uint2 particleUV = uint2(screenUV * _FoamRTSize.xy);
	InterlockedMax(_FoamBuffer[particleUV.y * _FoamRTSize.x + particleUV.x], posAlpha.a * BufferRange);
	InterlockedMax(_FoamLightBuffer[particleUV.y * _FoamRTSize.x + particleUV.x], encodedLight);
}
*/

void RenderFoam(uint waveIdx, uint idx, float time, float maxFrames, float exposure)
{
	ShorelineDataStruct waveData = KWS_ShorelineDataBuffer[waveIdx];
	time += waveData.timeOffset * KWS_SHORELINE_OFFSET_MULTIPLIER;
	float waveTime = frac(_FPS * time / maxFrames) * maxFrames ;

	ParticleData data = GetParticlePosAlpha(waveData, idx, waveTime);
	if (data.alpha < 0.0) return;

	float3 worldPos = mul(waveData.worldMatrix, float4(data.pos, 1.0)).xyz;
	float3 waterOffset = GetFftWavesDisplacement(worldPos);
	
	float2 orthoDepthUV = GetWaterOrthoDepthUV(worldPos + waterOffset);
	float terrainDepth = GetWaterOrthoDepth(orthoDepthUV);
	if (!IsOutsideUV(orthoDepthUV))
	{
		UNITY_BRANCH
		if(worldPos.y > terrainDepth + KWS_OrthoDepthPos.y + TERRAIN_DEPTH_OFFSET)
		{
			worldPos += waterOffset;
			worldPos.y = max(worldPos.y, terrainDepth + KWS_OrthoDepthPos.y + TERRAIN_DEPTH_OFFSET);
		}
		else
		{
			worldPos.y = max(worldPos.y, GetWaterOrthoDepth(GetWaterOrthoDepthUV(worldPos)) + KWS_OrthoDepthPos.y + TERRAIN_DEPTH_OFFSET);
		}
			
	}
	
	float idxHash = HashIdx(idx);

	#if defined(STEREO_INSTANCING_ON)
		for (int i = 0; i < 2; i++)
		{
			SetStereoIndex(i);
	#endif

	float4 screenPos = WorldPosToScreenPos(worldPos);
	float2 screenUV = screenPos.xy;
	float foamDepth = screenPos.z / screenPos.w;
	if (screenUV.x < 0.001 || screenUV.x > 0.999 || screenUV.y < 0.001 || screenUV.y > 0.999 || foamDepth < 0) return;
	
	float distanceToCamera = GetSurfaceDepth(screenPos.z);
	
	float depth = GetSceneDepth(screenUV);
	if (LinearEyeDepthUniversal(depth) < LinearEyeDepthUniversal(foamDepth)) return;

	float maxIdx = lerp(MAX_IDX_RANGE_PIXELS, 0, saturate(saturate(distanceToCamera * distanceToCamera * 0.006) + idxHash * 0.5));

	half3 lightColor = saturate(GetLight(worldPos, screenUV) * exposure);
	half3 sceneColor = GetSceneColorAfterWaterPass(screenUV);
	lightColor = lerp(sceneColor * lightColor * 0.75, lightColor, data.ao * data.ao);

	half3 fogColor;
	half3 fogOpacity;
	float3 viewDir = GetWorldSpaceViewDirNorm(worldPos);
	GetInternalFogVariables(float4(screenUV * _FoamRTSize.xy, foamDepth, 1), viewDir, LinearEyeDepthUniversal(foamDepth), screenPos.z, fogColor, fogOpacity);
	//	GetInternalFogVariables(i.pos, viewDir, surfaceDepthZ, i.screenPos.z, fogColor, fogOpacity);
	lightColor = ComputeInternalFog(lightColor, fogColor, fogOpacity);
	lightColor = ComputeThirdPartyFog(lightColor, worldPos, screenUV, screenPos.z);

	uint encodedLight = Pack_R11G11B10f(saturate(lightColor));
	uint hashOffset = GetBufferOffset;

	if (all(maxIdx < 1.0))
	{
		uint2 particleUV = uint2(screenUV * _FoamRTSize.xy);

		#if defined(KWS_FOAM_USE_FAST_MODE)
			InterlockedMax(_FoamRT[GetTextureID(particleUV)], Pack_R8G8B8A8n(saturate(float4(data.alpha, lightColor))));
		#else
			uint bufferUV = particleUV.y * _FoamRTSize.x + particleUV.x + hashOffset;
			InterlockedAdd(_FoamBuffer[bufferUV], data.alpha * BufferRange);
			InterlockedMax(_FoamLightBuffer[bufferUV], encodedLight);
		#endif
	}
	else
	{
		float doubleIndex = maxIdx * 2.0 + 0.00001;
		for (int y = 0; y < doubleIndex; ++y)
		{
			for (int x = 0; x < doubleIndex; ++x)
			{
				float2 uvOffset = float2(x, y) - maxIdx;
				uint2 particleUV = uint2((screenUV + uvOffset * _FoamRTSize.zw) * _FoamRTSize.xy);
				
				#if defined(KWS_FOAM_USE_FAST_MODE)
					InterlockedMax(_FoamRT[GetTextureID(particleUV)], Pack_R8G8B8A8n(saturate(float4(data.alpha, lightColor))));
				#else
					float normalizedDistanceView = saturate(distanceToCamera * 0.025);
					float cameraFade = lerp(1.0, 0.2, normalizedDistanceView);
					cameraFade = saturate(1.0 - length(uvOffset / maxIdx));
					uint bufferUV = particleUV.y * _FoamRTSize.x + particleUV.x + hashOffset;
					InterlockedAdd(_FoamBuffer[bufferUV], data.alpha * cameraFade * BufferRange);
					InterlockedMax(_FoamLightBuffer[bufferUV], encodedLight);
				#endif
			}
		}
	}

	#if defined(STEREO_INSTANCING_ON)
	}
	#endif
}


[numthreads(256, 1, 1)]
void ClearFoamBuffer(uint3 id : SV_DispatchThreadID)
{
	SetStereoIndex(id.z);
	uint hashOffset = GetBufferOffset;

	_FoamBuffer[id.y * _FoamRTSize.x + id.x + hashOffset] = 0;

	#ifndef KWS_FOAM_USE_FAST_MODE
		_FoamLightBuffer[id.y * _FoamRTSize.x + id.x + hashOffset] = 0;
	#endif
}


[numthreads(8, 8, 1)]
void RenderFoamToBuffer(uint3 id : SV_DispatchThreadID)
{
	uint idx = (id.y * _DispatchSize * 8.0 * _lodSizeMultiplier) + id.x * _lodSizeMultiplier;
	float totalTime = KW_Time * KW_GlobalTimeScale;

	#if defined(BAKE_AO)
		BakeShadowAO(0, idx, totalTime, 1.0);
	#else
		int waveIndex = _lodIndexes[_lodOffset +id.z];
		RenderFoam(waveIndex, idx, totalTime, 70, GetExposure());
	#endif
}


[numthreads(256, 1, 1)]
void RenderFoamBufferToTexture(uint3 id : SV_DispatchThreadID)
{
	#ifndef KWS_FOAM_USE_FAST_MODE
		SetStereoIndex(id.z);

		uint hashOffset = GetBufferOffset;
		uint bufferUV = id.y * _FoamRTSize.x + id.x + hashOffset;
		uint foam = _FoamBuffer[bufferUV];
		if (foam <= 1) return;

		uint data = _FoamLightBuffer[bufferUV];
		half3 encodedColor = Unpack_R11G11B10f(data);

		_FoamRT[GetTextureID(id.xy)] = half4(encodedColor, saturate(foam / BufferRange));
	#endif
}