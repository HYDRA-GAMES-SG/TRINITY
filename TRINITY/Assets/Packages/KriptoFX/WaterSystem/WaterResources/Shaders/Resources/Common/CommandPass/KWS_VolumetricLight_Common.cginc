#define CAUSTIC_LOD 1

uint KWS_Frame;
float KWS_VolumetricLightTemporalAccumulationFactor;
float2 KWS_VolumetricLightDownscaleFactor;

half MaxDistance;
uint KWS_RayMarchSteps;
half4 KWS_LightAnisotropy;

float KWS_VolumeLightMaxDistance;
float KWS_VolumeDepthFade;

struct RaymarchData
{
	float2 uv;
	float stepSize;
	float3 step;
	float offset;

	float3 currentPos;
	float3 rayStart;
	float3 rayEnd;
	float3 rayDir;
	float rayLength;
	float rayLengthToWaterZ;
	float rayLengthToSceneZ;
	bool surfaceMask;
	uint waterID;
	float waterHeight;
	float3 tubidityColor;
	float transparent;
	float causticStrength;
	float2 waterVolumeDepth;
};

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


vertexOutput vert(vertexInput v)
{
	vertexOutput o;
	UNITY_SETUP_INSTANCE_ID(v);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
	o.vertex = GetTriangleVertexPosition(v.vertexID);
	o.uv = GetTriangleUVScaled(v.vertexID);
	return o;
}

struct RaymarchResult
{
	float3 DirLightScattering;
	float DirLightSurfaceShadow;
	float DirLightSceneShadow;
	
	float3 AdditionalLightsScattering;
	float AdditionalLightsSceneAttenuation;
};

inline half MieScattering(float cosAngle)
{
	return KWS_LightAnisotropy.w * (KWS_LightAnisotropy.x / (KWS_LightAnisotropy.y - KWS_LightAnisotropy.z * cosAngle));
}

//inline float GetInDepthLightTransmitance(float waterHeight, float currentHeight, float transparent)
//{
//	float distanceToWaterSurface = max(0, (waterHeight - currentHeight));
//	float lightInDepthTransmitance = exp(-distanceToWaterSurface / transparent);
//	return lightInDepthTransmitance;
//}

inline float GetMaxRayDistanceRelativeToTransparent(float transparent)
{ 
    return min(KWS_MAX_TRANSPARENT, transparent * 1.5);
}

half RaymarchCaustic(RaymarchData raymarchData, float3 currentPos, float3 lightForward)
{
	float angle = dot(float3(0, -0.999, 0), lightForward);
	//float vericalOffset = GetCameraAbsolutePosition().y * 0.5 * (1-raymarchData.surfaceMask);
	float offsetLength = (raymarchData.waterHeight - currentPos.y) / angle;
	float2 uv = (currentPos.xz - offsetLength * lightForward.xz) / GetDomainSize(1, raymarchData.waterID);
	half caustic = GetCausticLod(uv, raymarchData.waterID, CAUSTIC_LOD) - KWS_CAUSTIC_MULTIPLIER;
	
	float causticOverScale = saturate(raymarchData.transparent * 0.15) * 0.25;
	caustic *= lerp(1, causticOverScale, raymarchData.surfaceMask);
	caustic *= clamp(raymarchData.causticStrength, 0, 5);

	float distanceToCamera = GetWorldToCameraDistance(currentPos);
	caustic = lerp(caustic, 0, saturate(distanceToCamera * 0.005));
	return caustic;
}

void IntegrateLightSlice(inout float3 finalScattering, inout float transmittance, float atten, RaymarchData raymarchData)
{
	float sliceDensity = KWS_VOLUME_LIGHT_SLICE_DENSITY / raymarchData.rayLength;
	float  sliceTransmittance = exp(-sliceDensity / (float)KWS_RayMarchSteps);
	float3 sliceLightIntegral = atten * (1.0 - sliceTransmittance);
	finalScattering += max(0, sliceLightIntegral * transmittance);
	transmittance *= sliceTransmittance;
}


RaymarchData InitRaymarchData(vertexOutput i, float waterMask)
{
	RaymarchData data;

	float sceneZ = GetSceneDepth(i.uv);
	data.waterVolumeDepth = GetWaterVolumeDepth(i.uv, sceneZ, waterMask);
	uint waterID = GetWaterID(i.uv);

	float3 startPos = GetWorldSpacePositionFromDepth(i.uv, data.waterVolumeDepth.x);
	float3 endPos = GetWorldSpacePositionFromDepth(i.uv, data.waterVolumeDepth.y);
	float2 ditherScreenPos = i.vertex.xy % 8;

	data.waterID = waterID;
	data.surfaceMask = GetSurfaceMask(waterMask);
	
	data.waterHeight = KWS_WaterPositionArray[waterID].y;
	data.tubidityColor = KWS_TurbidityColorArray[waterID];
	data.transparent = KWS_TransparentArray[waterID];
	data.transparent = clamp(data.transparent + KWS_UnderwaterTransparentOffsetArray[waterID] * (1-data.surfaceMask), 1, KWS_MAX_TRANSPARENT * 2);
	data.causticStrength = KWS_CausticStrengthArray[waterID];

	data.rayLength = GetMaxRayDistanceRelativeToTransparent(data.transparent);

	data.rayDir = normalize(endPos - startPos);
	data.rayLengthToWaterZ = length(startPos - endPos);
	data.rayLengthToSceneZ = length(startPos - GetWorldSpacePositionFromDepth(i.uv, sceneZ));
	data.rayStart = startPos;
	data.rayEnd = endPos;


	data.stepSize = data.rayLength / KWS_RayMarchSteps;
	data.step = data.rayDir * data.stepSize;
	data.offset = InterleavedGradientNoise(i.vertex.xy, KWS_Frame);

	data.currentPos = data.rayStart + data.step * data.offset;
	data.uv = i.uv;
	
	return data;
}

void AddTemporalAccumulation(float3 worldPos, inout float3 color)
{
	if (KWS_Frame > 5)
	{
		float2 reprojectedUV = WorldPosToScreenPosReprojectedPrevFrame(worldPos, 0).xy;
		float3 lastColor = GetVolumetricLightLastFrame(reprojectedUV).xyz;
		color.xyz = lerp(color, lastColor, KWS_VolumetricLightTemporalAccumulationFactor);
	}
}