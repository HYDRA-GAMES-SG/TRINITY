struct waterInput
{
	float4 vertex : POSITION;
	float3 surfaceMask : COLOR0;
	float3 normal : NORMAL;
	#if defined(USE_WATER_INSTANCING)
		float2 uvData : TEXCOORD0;
	#endif
	uint instanceID : SV_InstanceID;
};

struct v2fDepth
{
	float4 pos : SV_POSITION;
	float3 worldNormal : NORMAL;
	float3 worldPos : TEXCOORD0;
	float3 worldPosRefracted : TEXCOORD1;
	float3 surfaceMask : COLOR0;
	float4 screenPos : TEXCOORD2;
	float2 localHeightAndTensionMask: TEXCOORD32;
	UNITY_VERTEX_OUTPUT_STEREO
};

struct v2fWater
{
	float4 pos : SV_POSITION;
	float3 worldNormal : NORMAL;
	float3 surfaceMask : COLOR0;

	float3 worldPos : TEXCOORD0;
	float3 worldPosRefracted : TEXCOORD1;
	float4 screenPos : TEXCOORD2;

	UNITY_VERTEX_OUTPUT_STEREO
};


float3 ComputeWaterOffset(float3 worldPos)
{
	float3 offset = 0;

	float3 disp = 0;
	#if defined(KW_FLOW_MAP) || defined(KW_FLOW_MAP_EDIT_MODE) || defined(KW_FLOW_MAP_FLUIDS)
		disp = GetFftWavesDisplacementWithFlowMap(worldPos);
	#else
		disp = GetFftWavesDisplacement(worldPos);
	#endif
	
	#if KW_DYNAMIC_WAVES
		float dynamicWave = GetDynamicWavesDisplacement(worldPos);
		disp.y -= dynamicWave * 0.15;
	#endif

	//#if defined(KW_FLOW_MAP_FLUIDS)
	//	float2 fluidsUV_lod0 = (worldPos.xz - KW_FluidsMapWorldPosition_lod0.xz) / KW_FluidsMapAreaSize_lod0 + 0.5;
	//	float2 fluids_lod0 = KW_Fluids_Lod0.SampleLevel(sampler_linear_clamp, fluidsUV_lod0, 0).xy;

	//	float2 fluidsUV_lod1 = (worldPos.xz - KW_FluidsMapWorldPosition_lod1.xz) / KW_FluidsMapAreaSize_lod1 + 0.5;
	//	float2 fluids_lod1 = KW_Fluids_Lod1.SampleLevel(sampler_linear_clamp, fluidsUV_lod1, 0).xy;

	//	float2 maskUV_lod0 = 1 - saturate(abs(fluidsUV_lod0 * 2 - 1));
	//	float lodLevelFluidMask_lod0 = saturate((maskUV_lod0.x * maskUV_lod0.y - 0.01) * 3);
	//	float2 maskUV_lod1 = 1 - saturate(abs(fluidsUV_lod0 * 2 - 1));
	//	float lodLevelFluidMask_lod1 = saturate((maskUV_lod1.x * maskUV_lod1.y - 0.01) * 3);

	//	float2 fluids = lerp(fluids_lod1, fluids_lod0, lodLevelFluidMask_lod0);
	//	fluids *= lodLevelFluidMask_lod1;
	//	disp = ComputeDisplaceUsingFlowMap(KW_DispTex, sampler_linear_repeat, fluids * KW_FluidsVelocityAreaScale * 0.75, disp, uv, KW_Time * KW_FlowMapSpeed).xyz;
	//#endif
	

	#if USE_SHORELINE
		disp = ComputeShorelineOffset(worldPos, disp);
	#endif

	offset += disp;

	return offset;
}

float3 GetWaterOffsetRelativeToMask(float surfaceMask, float3 worldPos, float4x4 matrixIM)
{
	float3 waterOffset = ComputeWaterOffset(worldPos);
	return WorldToLocalPosWithoutTranslation(waterOffset, matrixIM) * surfaceMask;
}

float3 GetWaterOffsetRelativeToMaskInstanced(uint instanceID, float surfaceMask, float3 worldPos, float uvDataX, float4x4 matrixIM)
{
#if defined (USE_WATER_INSTANCING)
	InstancedMeshDataStruct meshData = InstancedMeshData[instanceID];
	uint mask = (uint)uvDataX;
	float down = GetFlag(mask, 5) * meshData.downInf;
	float left = GetFlag(mask, 6) * meshData.leftInf;
	float top = GetFlag(mask, 7) * meshData.topInf;
	float right = GetFlag(mask, 8) * meshData.rightInf;
	if (!down && !left && !top && !right) surfaceMask = 1;
	
	#ifndef USE_WATER_TESSELATION
		if (worldPos.y < KW_WaterPosition.y) surfaceMask = 0;
	#endif

	return GetWaterOffsetRelativeToMask(surfaceMask, worldPos, matrixIM);
#else 
	return 0;
#endif
}

v2fDepth vertDepth(waterInput v)
{
	v2fDepth o = (v2fDepth)0;
	
	UNITY_SETUP_INSTANCE_ID(v);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
	KWS_INITIALIZE_DEFAULT_MATRIXES;

	#if defined(USE_WATER_INSTANCING) &&  !defined(USE_WATER_TESSELATION)
		UpdateInstanceData(v.instanceID, v.uvData, v.vertex, KWS_MATRIX_M, KWS_MATRIX_I_M);
	#endif

	o.worldPos.xyz = LocalToWorldPos(v.vertex.xyz, KWS_MATRIX_M);
	//o.surfaceMask = lerp(v.surfaceMask, 1, KWS_MeshType == KWS_MESH_TYPE_CUSTOM_MESH);
	o.surfaceMask = v.surfaceMask;

	float3 offset = 0;
	#if defined(USE_WATER_INSTANCING)
		offset += GetWaterOffsetRelativeToMaskInstanced(v.instanceID, o.surfaceMask.x, o.worldPos.xyz, v.uvData.x, KWS_MATRIX_I_M);
	#else
		offset += GetWaterOffsetRelativeToMask(o.surfaceMask.x, o.worldPos.xyz, KWS_MATRIX_I_M);
	#endif

	
	//#ifdef KWS_VOLUME_MASK
	//	float2 volumeData = GetWaterVolumePrePassVertex(LocalToWorldPos(v.vertex.xyz + offset, KWS_MATRIX_M));
	//	WaterVolumeData waterData = GetWaterVolumeData((uint)volumeData.y);
	//	if(volumeData.x < 1) v.vertex.y = waterData.WaterLevel;
	//#endif
	v.vertex.xyz += offset;

	#if defined(KWS_USE_EXTRUDE_MASK)
		v.vertex.xyz += ComputeExtrudeMask(LocalToWorldPos(v.vertex.xyz, KWS_MATRIX_M), o.surfaceMask);
	#endif

	o.worldPosRefracted.xyz = LocalToWorldPos(v.vertex.xyz, KWS_MATRIX_M);
	o.localHeightAndTensionMask.x = offset.y;
	o.localHeightAndTensionMask.y = v.vertex.y - offset.y;

	o.pos = ObjectToClipPos(v.vertex, KWS_MATRIX_M, KWS_MATRIX_I_M);
	o.screenPos = ComputeScreenPos(o.pos);
	o.worldNormal = GetWorldSpaceNormal(v.normal, KWS_MATRIX_M);
	return o;
}

v2fWater vertWater(waterInput v)
{
	v2fWater o = (v2fWater)0;

	UNITY_SETUP_INSTANCE_ID(v);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
	KWS_INITIALIZE_DEFAULT_MATRIXES;

	#if defined(USE_WATER_INSTANCING) &&  !defined(USE_WATER_TESSELATION)
		UpdateInstanceData(v.instanceID, v.uvData, v.vertex, KWS_MATRIX_M, KWS_MATRIX_I_M);
	#endif
	o.worldPos = LocalToWorldPos(v.vertex.xyz, KWS_MATRIX_M);
	//o.surfaceMask = lerp(v.surfaceMask, 1, KWS_MeshType == KWS_MESH_TYPE_CUSTOM_MESH);
	o.surfaceMask = v.surfaceMask;

	float3 offset = 0;
	#if defined(USE_WATER_INSTANCING)
		offset += GetWaterOffsetRelativeToMaskInstanced(v.instanceID, o.surfaceMask.x, o.worldPos.xyz, v.uvData.x, KWS_MATRIX_I_M);
	#else
		offset += GetWaterOffsetRelativeToMask(o.surfaceMask.x, o.worldPos.xyz, KWS_MATRIX_I_M);
	#endif

	//#ifdef KWS_VOLUME_MASK
	//	float2 volumeData = GetWaterVolumePrePassVertex(LocalToWorldPos(v.vertex.xyz + offset, KWS_MATRIX_M));
	//	WaterVolumeData waterData = GetWaterVolumeData((uint)volumeData.y);
	//	if(volumeData.x < 1) v.vertex.y = waterData.WaterLevel;

	//#endif
	v.vertex.xyz += offset;

	#if defined(KWS_USE_EXTRUDE_MASK)
		v.vertex.xyz += ComputeExtrudeMask(LocalToWorldPos(v.vertex.xyz, KWS_MATRIX_M), o.surfaceMask);
	#endif

	#ifndef USE_WATER_TESSELATION
		if (KWS_UseWireframeMode)
		{
			o.surfaceMask = ComputeWireframeInterpolators(v.surfaceMask.z);
		}
	#endif

	
	o.worldPosRefracted = LocalToWorldPos(v.vertex.xyz, KWS_MATRIX_M);
	o.pos = ObjectToClipPos(v.vertex, KWS_MATRIX_M, KWS_MATRIX_I_M);
	o.screenPos = ComputeScreenPos(o.pos);
	o.worldNormal = GetWorldSpaceNormal(v.normal, KWS_MATRIX_M);
	return o;
}