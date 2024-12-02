#ifndef KWS_WATER_VARIABLES
#define KWS_WATER_VARIABLES

#if defined(STEREO_INSTANCING_ON)
	#define DECLARE_TEXTURE(tex) Texture2DArray tex
	#define DECLARE_TEXTURE_UINT(tex) Texture2DArray<uint> tex
	#define SAMPLE_TEXTURE(tex, samplertex, uv) tex.Sample(samplertex, float3(uv, (float)unity_StereoEyeIndex))
	#define SAMPLE_TEXTURE_LOD(tex, samplertex, uv, lod) tex.SampleLevel(samplertex, float3(uv, (float)unity_StereoEyeIndex), lod)
	#define SAMPLE_TEXTURE_LOD_OFFSET(tex, samplertex, uv, lod, offset) tex.SampleLevel(samplertex, float3(uv, (float)unity_StereoEyeIndex), lod, offset)
	#define SAMPLE_TEXTURE_GATHER(tex, samplertex, uv) tex.Gather(samplertex, float3(uv, (float)unity_StereoEyeIndex))
	#define SAMPLE_TEXTURE_GATHER_GREEN(tex, samplertex, uv) tex.GatherGreen(samplertex, float3(uv, (float)unity_StereoEyeIndex))
	#define SAMPLE_TEXTURE_GATHER_BLUE(tex, samplertex, uv) tex.GatherBlue(samplertex, float3(uv, (float)unity_StereoEyeIndex))
	#define SAMPLE_TEXTURE_GATHER_ALPHA(tex, samplertex, uv) tex.GatherAlpha(samplertex, float3(uv, (float)unity_StereoEyeIndex))
	#define LOAD_TEXTURE(tex, uv) tex.Load(uint4(uv, unity_StereoEyeIndex, 0))
	#define LOAD_TEXTURE_OFFSET(tex, uv, offset) tex.Load(uint4(uv, unity_StereoEyeIndex, 0), offset)
	#define SAMPLE_TEXTURE_GRAD(tex, samplertex, uv, dx, dy) tex.SampleGrad(samplertex, float3(uv, (float)unity_StereoEyeIndex), dx, dy)
#else
	#define DECLARE_TEXTURE(tex) Texture2D tex
	#define DECLARE_TEXTURE_UINT(tex) Texture2D<uint> tex
	#define SAMPLE_TEXTURE(tex, samplertex, uv) tex.Sample(samplertex, uv)
	#define SAMPLE_TEXTURE_LOD(tex, samplertex, uv, lod) tex.SampleLevel(samplertex, uv, lod)
	#define SAMPLE_TEXTURE_LOD_OFFSET(tex, samplertex, uv, lod, offset) tex.SampleLevel(samplertex, uv, lod, offset)
	#define SAMPLE_TEXTURE_GATHER(tex, samplertex, uv) tex.Gather(samplertex, uv)
	#define SAMPLE_TEXTURE_GATHER_GREEN(tex, samplertex, uv) tex.GatherGreen(samplertex, uv)
	#define SAMPLE_TEXTURE_GATHER_BLUE(tex, samplertex, uv) tex.GatherBlue(samplertex, uv)
	#define SAMPLE_TEXTURE_GATHER_ALPHA(tex, samplertex, uv) tex.GatherAlpha(samplertex, uv)
	#define LOAD_TEXTURE(tex, uv) tex.Load(uint3(uv, 0))
	#define LOAD_TEXTURE_OFFSET(tex, uv, offset) tex.Load(uint3(uv, 0), offset)
	#define SAMPLE_TEXTURE_GRAD(tex, samplertex, uv, dx, dy) tex.SampleGrad(samplertex, uv, dx, dy)
#endif

float4x4 KWS_MATRIX_VP_STEREO[2];
float4x4 KWS_PREV_MATRIX_VP_STEREO[2];
float4x4 KWS_MATRIX_I_VP_STEREO[2];
float4x4 KWS_MATRIX_CAMERA_PROJECTION_STEREO[2];
float4x4 KWS_MATRIX_I_P_STEREO[2];

float4x4 KWS_MATRIX_VP;
float4x4 KWS_PREV_MATRIX_VP;
float4x4 KWS_MATRIX_I_VP;
float4x4 KWS_MATRIX_CAMERA_PROJECTION;
float4x4 KWS_MATRIX_I_P;

float4x4 KW_ViewToWorld;
float4x4 KW_ProjToView;

float4x4 KWS_MATRIX_VP_ORTHO;
float3 KWS_WorldSpaceCameraPosOrtho;

#if defined(STEREO_INSTANCING_ON)
	#define KWS_MATRIX_VP KWS_MATRIX_VP_STEREO[unity_StereoEyeIndex]
	#define KWS_PREV_MATRIX_VP KWS_PREV_MATRIX_VP_STEREO[unity_StereoEyeIndex]
	#define KWS_MATRIX_I_VP KWS_MATRIX_I_VP_STEREO[unity_StereoEyeIndex]
	#define KWS_MATRIX_CAMERA_PROJECTION KWS_MATRIX_CAMERA_PROJECTION_STEREO[unity_StereoEyeIndex]
	#define KWS_MATRIX_I_P KWS_MATRIX_I_P_STEREO[unity_StereoEyeIndex]
#else
	#define KWS_MATRIX_VP KWS_MATRIX_VP
	#define KWS_PREV_MATRIX_VP KWS_PREV_MATRIX_VP
	#define KWS_MATRIX_I_VP KWS_MATRIX_I_VP
	#define KWS_MATRIX_CAMERA_PROJECTION KWS_MATRIX_CAMERA_PROJECTION
	#define KWS_MATRIX_I_P KWS_MATRIX_I_P
#endif

#define NAN_VALUE asfloat(0x7fc00000)
#define INF_VALUE asfloat(0x7F800000)
#define UINT_MAX_VALUE 4294967295.0

#define MAX_WATER_INSTANCES 32
#define MIN_THRESHOLD 0.00001
#define KWS_SHORELINE_OFFSET_MULTIPLIER 34.0
#define KWS_WATER_MASK_DECODING_VALUE 1.0/255.0
#define KWS_WATER_MASK_ENCODING_VALUE 255.0
#define KWS_WATER_SSR_NORMAL_LOD 3.0

#define KWS_MAX_TRANSPARENT 50.0
#define KWS_MAX_WIND_SPEED 50.0
#define FOAM_MIN_WIND 7

#define KWS_MESH_TYPE_INFINITE_OCEAN 0
#define KWS_MESH_TYPE_FINITE_BOX 1
#define KWS_MESH_TYPE_RIVER 2
#define KWS_MESH_TYPE_CUSTOM_MESH 3

#define KWS_CAUSTIC_MULTIPLIER 0.15
#define KWS_VOLUME_LIGHT_SLICE_DENSITY 10
#define KWS_WATER_IOR 1.33

SamplerState sampler_point_clamp;
SamplerState sampler_point_repeat;
SamplerState sampler_linear_repeat;
SamplerState sampler_linear_clamp;
SamplerState sampler_trilinear_clamp;
SamplerState sampler_trilinear_repeat;

float4 KWS_WaterViewPort;

half4 _MainColor;
half4 _SeaColor;
half4 _BottomColor;
half4 _BottomColorDeep;
half4 _SSSColor;
half4 _DiffColor;
half4 _BubblesColor;
half4 _IndirectDiffColor;
half4 _IndirectSpecColor;
half _Metalic;
half _Roughness;


Texture2D					KW_DispTex;
Texture2D					KW_DispTex_LOD1;
Texture2D					KW_DispTex_LOD2;
Texture2D					KW_NormTex;
SamplerState sampler_KW_NormTex;
Texture2D					KW_NormTex_LOD1;
Texture2D					KW_NormTex_LOD2;

Texture2D KW_FluidsDepthTex;
float KW_FluidsDepthOrthoSize;
float3 KW_FluidsDepth_Near_Far_Dist;
float3 KW_FluidsDepthPos;


sampler2D KW_RipplesTexture;
sampler2D KW_RipplesTexturePrev;
sampler2D KW_RipplesNormalTexture;
sampler2D KW_RipplesNormalTexturePrev;
sampler2D _ReflectionTex;

sampler2D KW_WaterOpaqueTexture;
//sampler2D					_ShadowMapTexture;

sampler2D KW_WaterMaskDepth;

float4 KW_DispTex_TexelSize;
float4 KW_DispTexDetail_TexelSize;
float4 KW_NormTex_TexelSize;
float4 KW_NormTex_LOD1_TexelSize;
float4 KW_NormTex_LOD2_TexelSize;
float4 KW_DispTex_LOD1_TexelSize;
float4 KW_DispTex_LOD2_TexelSize;



float _Distortion;
float _test;
float4 _test2;
float _test3;

half _Turbidity;
half _WaterTimeScale;
half KW_FFTDomainSize;
half KW_FFTDomainSize_LOD1;
half KW_FFTDomainSize_LOD2;
half KW_FFTDomainSize_Detail;


float2 KW_RipplesUVOffset;
half KW_RipplesScale;



sampler2D KW_DitherTexture;
sampler2D KW_DistanceFieldDepthIntersection;
sampler2D KW_DistanceField;
sampler2D KW_TimeRemap;
sampler2D KW_ShoreWaveTex;
sampler2D KW_UpDepth;
float4 KW_DistanceFieldPos;
float4 KW_UpDepthPos;

sampler2D KW_BeachWavesTex;
float4 KW_BeachWavesPos;

uniform float4 _GAmplitude;
uniform float4 _GFrequency;
uniform float4 _GSteepness;
uniform float4 _GSpeed;
uniform float4 _GDirectionAB;
uniform float4 _GDirectionCD;


sampler2D _CameraDepthTextureBeforeWaterZWrite;
sampler2D _CameraDepthTextureBeforeWaterZWrite_Blured;
float4 _CameraDepthTextureBeforeWaterZWrite_TexelSize;


half _DistanceBetweenBeachWaves;
half _MinimalDepthForBeachWaves;

float3 KW_DirLightForward;
float3 KW_DirLightColor;

int KW_PointLightCount;
float4 KW_PointLightPositions[100];
float4 KW_PointLightColors[100];


float3 KW_DynamicWavesWorldPos;
float3 KW_InteractCameraOffset_Last;
sampler2D KW_InteractiveWavesNormalTex;
sampler2D KW_InteractiveWavesNormalTexPrev;
sampler2D KW_ShorelineTex;
sampler2D KW_ShorelineNormalTex;
float4 KW_ShorelineTex_TexelSize;

sampler2D KW_ShorelineTexMap;
float KW_ShorelineSize;
float3 KW_ShorelineOffset;

float3 KW_DistanceFieldDepthPos;
float KW_DistanceFieldDepthArea;
float KW_DistanceFieldDepthFar;

sampler2D _TestTexture;
sampler2D _TestDispTexture;
sampler2D _TestNormalTexture;
float4 FoamAnimUV;

sampler2D KW_ReflectionTex;

float KW_DepthOrthographicSize;
float4 Test4;

float3 KW_FluidsMapWorldPosition_lod0;
float3 KW_FluidsMapWorldPosition_lod1;
float3 KWS_FluidsMaskWorldPosition;
float KW_FluidsMapAreaSize_lod0;
float KW_FluidsMapAreaSize_lod1;

Texture2D KW_Fluids_Lod0;
Texture2D KW_FluidsFoam_Lod0;
Texture2D KWS_FluidsMask_Lod0;

Texture2D KW_Fluids_Lod1;
Texture2D KW_FluidsFoam_Lod1;
Texture2D KWS_FluidsMask_Lod1;

Texture2D KW_FluidsFoamTex;

Texture2D KW_FluidsFoamTexBubbles;
float KW_FluidsVelocityAreaScale;

float srpBatcherFix;
float3 KWS_CameraForward;

Texture2D KWS_OceanFoamTex;
Texture2D KWS_IntersectionFoamTex;

///////////////////////////////////// Arrays ////////////////////////////////////////////////////////
float KWS_WaterInstancesCount;
float KWS_WindSpeedArray[MAX_WATER_INSTANCES];

float KWS_TransparentArray[MAX_WATER_INSTANCES];
float KWS_UnderwaterTransparentOffsetArray[MAX_WATER_INSTANCES];
float3 KWS_DyeColorArray[MAX_WATER_INSTANCES];
float3 KWS_TurbidityColorArray[MAX_WATER_INSTANCES];

float3 KWS_WaterPositionArray[MAX_WATER_INSTANCES];
float KWS_CausticStrengthArray[MAX_WATER_INSTANCES];
uint KWS_MeshTypeArray[MAX_WATER_INSTANCES];
float KWS_WavesAreaScaleArray[MAX_WATER_INSTANCES];
////////////////////////////////////////////////////////////////////////////////////////////////////


///////////////////////////////////// Dynamic data ////////////////////////////////////////////////////////
float KW_Time;
float KWS_ScaledTime;
float3 KW_WaterPosition;
uint KWS_WaterInstanceID;
int KWS_UnderwaterVisible;
int KWS_FogState;
//uint KWS_UseClipMask;

float3 KWS_AmbientColor;
float3 KWS_DirLightDireciton;
float3 KWS_DirLightColor;

float KWS_OceanLevel;
float3 KWS_WaterWorldPosOffset;
///////////////////////////////////////////////////////////////////////////////////////////////////



///////////////////////////////////// Constant data ////////////////////////////////////////////////////////
uint KWS_WaterLayerMask;
uint KWS_WaterLightLayerMask;

float3 KW_TurbidityColor;
float3 KWS_DyeColor;


float KW_GlobalTimeScale;
float KWS_SunMaxValue;
float KW_WaterFarDistance;
float KW_FFT_Size_Normalized;
float KW_Transparent;
float KW_Turbidity;
float KWS_SunCloudiness;
float KWS_SunStrength;
float KWS_RefractionAproximatedDepth;
float KWS_RefractionSimpleStrength;
float KWS_RefractionDispersionStrength;
float _TesselationFactor;
float _TesselationMaxDistance;
float _TesselationMaxDisplace;
float KWS_ReflectionClipOffset;
float3 KWS_CustomSkyColor;
float KWS_AnisoReflectionsScale;
float KWS_SkyLodRelativeToWind;
float KW_FlowMapSize;
float KW_FlowMapSpeed;
float KW_FlowMapFluidsStrength;
float4 KW_FlowMapOffset;
float KWS_UnderwaterHalfLineTensionScale;

float2 KWS_OceanFoamStrengthSize;
float4 KWS_OceanFoamColor;

float2 KWS_IntersectionFoamFadeSize;
float4 KWS_IntersectionFoamColor;


uint KWS_OverrideSkyColor;
//uint KWS_IsCustomMesh;

uint KWS_UseOceanFoam;
uint KWS_UseIntersectionFoam;
uint KWS_UseWireframeMode;
uint KWS_UseMultipleSimulations;
uint KWS_UseRefractionIOR;
uint KWS_UseRefractionDispersion;
int KWS_MeshType;
uint KWS_UseFilteredNormals;

float KWS_AbsorbtionOverrideMultiplier;
///////////////////////////////////////////////////////////////////////////////////////////////////



/////////////////////////////////////  Instanced data ///////////////////////////////////////////////////
float KWS_WaterYRotationRad;
float3 KWS_InstancingWaterScale;
//uint KWS_IsFiniteTypeInstancing;

#if defined (USE_WATER_INSTANCING)

	struct InstancedMeshDataStruct
	{
		float3 position;
		float3 size;

		uint downSeam;
		uint leftSeam;
		uint topSeam;
		uint rightSeam;

		uint downInf;
		uint leftInf;
		uint topInf;
		uint rightInf;
	};
	StructuredBuffer<InstancedMeshDataStruct> InstancedMeshData;

#endif


///////////////////////////////////////////////////////////////////////////////////////////////////////////////


/////////////////////////////////////  Shoreline variables ///////////////////////////////////////////////////


//////////////////////////////////////////////////////////////////////////////////////////////////////////////



//////////////////////////////////////// Ortho depth variables //////////////////////////////////////////////

/////////////////////////////////////////////////////////////////////////////////////////////////////////////




//////////////////////////////////////// Reflection probes variables //////////////////////////////////////////////
TextureCube KWS_SpecCube0;
float4 KWS_SpecCube0_HDR;
float4 KWS_SpecCube0_BoxMax;
float4 KWS_SpecCube0_BoxMin;
float4 KWS_SpecCube0_ProbePosition;

TextureCube KWS_SpecCube1;
float4 KWS_SpecCube1_HDR;
float4 KWS_SpecCube1_BoxMax;
float4 KWS_SpecCube1_BoxMin;
float4 KWS_SpecCube1_ProbePosition;

//float KWS_ReflectionProbeTileSize;
float2 KWS_ReflectionProbeTilesCount;
float KWS_VisibleReflectionProbesCount;

//struct ReflectionProbeData
//{
//	//dont forget about padding
//	float3 MinBounds;
//	float Intensity;
	
//	float3 MaxBounds;
//	float BlendDistance;

//	float Importance;
//	float3 _pad;
//};

//StructuredBuffer<ReflectionProbeData> KWS_ReflectionProbeData;
//StructuredBuffer<uint> KWS_ReflectionProbeTileList;

//inline uint GetReflectionProbeID(float2 uv)
//{
//	uint2 id = uv * KWS_ReflectionProbeTilesCount;
//	uint tileID = id.y * KWS_ReflectionProbeTilesCount.x + id.x;
//	return KWS_ReflectionProbeTileList[tileID];
//}
/////////////////////////////////////////////////////////////////////////////////////////////////////////////


#if defined (KWS_USE_EXTRUDE_MASK)

	#ifndef KWS_WATERVOLUMEVARIABLES_CS_HLSL
		#include "KWS_WaterVolumeVariables.hlsl"
	#endif

	StructuredBuffer<WaterVolumeData> KWS_WaterVolumeDataBuffer;
	uint KWS_WaterVolumeDataBufferLength;
	//#define GetWaterVolumeData(id) KWS_WaterVolumeDataBuffer[id - 1] //waterID always > 0, because 0 is non-water area

	#endif


#endif