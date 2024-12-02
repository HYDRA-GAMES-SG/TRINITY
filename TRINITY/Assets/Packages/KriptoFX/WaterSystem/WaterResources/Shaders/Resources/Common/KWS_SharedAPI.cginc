#ifndef KWS_SHARED_API_INCLUDED

#define KWS_SHARED_API_INCLUDED


#ifndef SHADERGRAPH_PREVIEW

	#include "../PlatformSpecific/KWS_PlatformSpecificHelpers.cginc"
	#include "../Common/KWS_WaterPassHelpers.cginc"
	#include "../Common/KWS_WaterHelpers.cginc"

#endif

float3 KWS_ParticlesPos; //particle system transform world position
float3 KWS_ParticlesScale; //particle system transform localScale


inline float3 TileWarpParticlesOffsetXZ(float3 vertex, float3 center)
{
	float3 halfScale = KWS_ParticlesScale * 0.5;
	float3 quadOffset = vertex.xyz - center.xyz;
	vertex.xz = frac((center.xz + halfScale.xz - KWS_ParticlesPos.xz) / KWS_ParticlesScale.xz) * KWS_ParticlesScale.xz; //aabb warp
	vertex.xz += KWS_ParticlesPos.xz + quadOffset.xz - halfScale.xz; //ofset relative to pivot and size
	return vertex;
}


inline float3 GetWaterSurfaceCollisionForQuadParticlesAquarium(float3 vertex, float3 center, float levelOffset)
{
	#ifdef SHADERGRAPH_PREVIEW
		return vertex;
	#else
		
		float waterLevel = KWS_ParticlesPos.y + levelOffset; 
		float3 waterDisplacement = GetFftWavesDisplacement(vertex);
		//vertex.xyz += ComputeExtrudeMask(vertex);


		float3 quadOffset = vertex.xyz - center.xyz;
		float quadOffsetLength = length(quadOffset);

		float currentOffset = 0;
		float currentScale = 1;

		if (center.y > waterLevel - quadOffsetLength)
		{
			center.y = waterLevel + waterDisplacement.y - quadOffsetLength;
			vertex = center.xyz + quadOffset;
		}
		
		return vertex;
	#endif
}

inline float3 GetWaterSurfaceCollisionForQuadParticles(float3 vertex, float3 center)
{
	#ifdef SHADERGRAPH_PREVIEW
		return vertex;
	#else
		
		float4 screenPos = ComputeScreenPos(ObjectToClipPos(float4(vertex, 1)));
		float2 screenUV = screenPos.xy / screenPos.w;
		uint waterID = GetWaterID(screenUV);
		bool underwaterMask = GetUnderwaterMask(GetWaterMask(screenUV));
		
		if (waterID == 0 || !underwaterMask)
		{
			vertex = NAN_VALUE;
			return vertex;
		}
		vertex.y += ComputeExtrudeMask(vertex).y * 0.5;

	
		float3 waterPos = KWS_WaterPositionArray[waterID];
		float3 waterDisplacement = GetFftWavesDisplacement(vertex);
		
	

		float3 quadOffset = vertex.xyz - center.xyz;
		float quadOffsetLength = length(quadOffset);
		
		if (center.y > waterPos.y - quadOffsetLength)
		{
			center.y = waterPos.y + waterDisplacement.y - quadOffsetLength;
			vertex = center.xyz + quadOffset;
		}
		
		return vertex;
	#endif
}


inline float4 GetUnderwaterColorAlbedo(float2 uv, float3 albedoColor, float3 vertexWorldPos)
{
	#ifdef SHADERGRAPH_PREVIEW
		return 1;
	#else

		float2 underwaterUV = clamp(uv, 0.01, 0.99);
		
		uint waterID = GetWaterID(uv);

		float transparent = KWS_TransparentArray[waterID];
		float3 turbidityColor = KWS_TurbidityColorArray[waterID];
		float3 dyeColor = KWS_DyeColorArray[waterID];

		float distanceToVertex = GetWorldToCameraDistance(vertexWorldPos);
		float3 volLight = GetVolumetricLightWithAbsorbtionByDistance(uv, uv, transparent, turbidityColor, dyeColor, 0, distanceToVertex,  waterID, GetExposure(), albedoColor * 5).xyz;

		return half4(volLight, 1);

	#endif
}

inline float4 GetUnderwaterColorRefraction(float2 uv, float3 albedoColor, float2 refractionNormal, float3 vertexWorldPos)
{
	#ifdef SHADERGRAPH_PREVIEW
		return 1;
	#else
		float2 underwaterUV = clamp(uv + refractionNormal, 0.01, 0.99);
		
		half3 refraction = GetSceneColor(underwaterUV);
		uint waterID = GetWaterID(uv);

		float transparent = KWS_TransparentArray[waterID];
		float3 turbidityColor = KWS_TurbidityColorArray[waterID];
		float3 dyeColor = KWS_DyeColorArray[waterID];

		float distanceToVertex = GetWorldToCameraDistance(vertexWorldPos) + 2;
		
		float3 volLight = GetVolumetricLightWithAbsorbtionByDistance(uv, uv, transparent, turbidityColor, dyeColor, refraction, distanceToVertex, waterID, GetExposure(), albedoColor * 5).xyz;
		return half4(volLight, 1);
		
	#endif
}

inline float4 GetUnderwaterColorRefractionAquarium(float2 uv, float3 albedoColor, float2 refractionNormal)
{
	#ifdef SHADERGRAPH_PREVIEW
		return 1;
	#else
		float2 underwaterUV = clamp(uv + refractionNormal, 0.01, 0.99);
		half3 refraction = GetSceneColor(underwaterUV);
	
		refraction += albedoColor;
		return half4(refraction, 1);
		
	#endif
}

////////////////////////////// shadergraph support /////////////////////////////////////////////////////////////////////

inline void GetDecalVertexOffset_float(float3 worldPos, float displacement, out float3 result)
{
	#ifdef SHADERGRAPH_PREVIEW
		result = 0;
	#else
		float3 extrudeOffset = ComputeExtrudeMask(GetAbsolutePositionWS(worldPos)) * 0.5;
		result = worldPos + GetFftWavesDisplacement(GetAbsolutePositionWS(worldPos)) * saturate(float3(displacement, 1, displacement)) + extrudeOffset;
	#endif
}



inline void GetDecalDepthTest_float(float4 screenPos, out float result)
{
	#ifdef SHADERGRAPH_PREVIEW
		result = 1;
	#else
		float sceneDepth = GetSceneDepth(screenPos.xy / screenPos.w);
		result = LinearEyeDepthUniversal(sceneDepth) > LinearEyeDepthUniversal(screenPos.z / screenPos.w);
	#endif
}


inline void TileWarpParticlesOffsetXZ_float(float3 vertex, float3 center, out float3 result)
{
	result = TileWarpParticlesOffsetXZ(vertex, center);
}


inline void GetWaterSurfaceCollisionForQuadParticles_float(float3 vertex, float3 center, out float3 result)
{
	result = GetWaterSurfaceCollisionForQuadParticles(vertex, center);
}

inline void GetWaterSurfaceCollisionForQuadParticlesAquarium_float(float3 vertex, float3 center, float levelOffset, out float3 result)
{
	result = GetWaterSurfaceCollisionForQuadParticlesAquarium(vertex, center, levelOffset);
}

void GetUnderwaterColorRefraction_float(float2 uv, float3 albedoColor, float2 refractionNormal, float3 worldPos, out float4 result) //shadergraph function

{
	result = GetUnderwaterColorRefraction(uv, albedoColor, refractionNormal, worldPos);
}

void GetUnderwaterColorRefractionAquarium_float(float2 uv, float3 albedoColor, float2 refractionNormal, out float4 result) //shadergraph function

{
	result = GetUnderwaterColorRefractionAquarium(uv, albedoColor, refractionNormal);
}


void GetUnderwaterColorAlbedo_float(float2 uv, float3 albedoColor, float3 worldPos, out float4 result) //shadergraph function

{
	result = GetUnderwaterColorAlbedo(uv, albedoColor, worldPos);
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////




#endif