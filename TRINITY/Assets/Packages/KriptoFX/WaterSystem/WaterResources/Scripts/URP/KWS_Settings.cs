using UnityEngine;
using System.Collections.Generic;
using static KWS.WaterSystemScriptableData;
namespace KWS
{
    internal static class KWS_Settings
    {
        public static readonly int MaskStencilValue = 8; //builtin 32, urp 8, hdrp 128

        public static class Water
        {
            public static readonly int DefaultWaterQueue             = 3000;
            public static readonly int ShorelineQueueOffset          = +1;
            public static readonly int UnderwaterQueueOffset         = +1;
            public static readonly int WaterLayer                    = 4; //water layer bit mask
            public static readonly int LightLayer                    = 1; //light layer bit mask used in urp/hdrp

            public static readonly int OrthoDepthResolution = 2048;
            public static readonly int OrthoDepthAreaSize = 200;
            public static readonly float OrthoDepthAreaFarOffset = 100;
            public static readonly float OrthoDepthAreaNearOffset = 2;

            public static readonly int MaxRefractionDispersion = 5;

            public static readonly Dictionary<WaterMeshQualityEnum, int[]> QuadTreeChunkQuailityLevelsInfinite = new Dictionary<WaterMeshQualityEnum, int[]>()
            {
                {WaterMeshQualityEnum.Ultra, new[] {4, 6, 8, 12, 16, 20}},
                {WaterMeshQualityEnum.High, new[] {2, 4, 6, 8, 12, 16}},
                {WaterMeshQualityEnum.Medium, new[] {1, 2, 4, 6, 8, 12}},
                {WaterMeshQualityEnum.Low, new[] {1, 2, 3, 4, 6, 8}},
                {WaterMeshQualityEnum.VeryLow, new[] {1, 2, 3, 4, 5, 6}}
            };

            public static readonly Dictionary<WaterMeshQualityEnum, int[]> QuadTreeChunkQuailityLevelsFinite = new Dictionary<WaterMeshQualityEnum, int[]>()
            {
                {WaterMeshQualityEnum.Ultra, new[] {2, 4, 6, 8, 10, 12}},
                {WaterMeshQualityEnum.High, new[] {2, 4, 5, 6, 8, 10}},
                {WaterMeshQualityEnum.Medium, new[] {1, 2, 3, 4, 6, 8}},
                {WaterMeshQualityEnum.Low, new[] {1, 2, 3, 4, 5, 6}},
                {WaterMeshQualityEnum.VeryLow, new[] {1, 1, 2, 3, 4, 5}}
            };

            public static readonly float[] QuadTreeChunkLodRelativeToWind = { 0.5f, 0.75f, 1f, 1.5f, 2f, 2.5f };
            public static readonly int QuadTreeChunkLodOffsetForDynamicWaves = 2;

            public static readonly bool IsPostfxRequireDepthWriting = true;
        }

        public static class ResourcesPaths
        {
            public const string WaterSettingsProfileAssetName = "WaterSettings";

            public static readonly string KWS_FluidsFoamTex = "Textures/FluidsFoamTex";
            public static readonly string KWS_IntersectionFoamTex = "Textures/IntersectionFoamTex";
            public static readonly string KWS_OceanFoamTex = "Textures/OceanFoamTex";

            public static readonly string KWS_DefaultVideoLoading = "Textures/KWS_DefaultVideoLoading";
            public static readonly string ShorelineAlpha = "Textures/ShorelineAlpha";
            public static readonly string ShorelineNorm = "Textures/ShorelineNorm";
            public static readonly string ShorelinePos = "Textures/ShorelinePos";

            public static readonly string FoamData = "Binary/FoamData";
            public static readonly string FoamDataCountOffset = "Binary/FoamDataCountOffset";

            public static readonly string BinaryFlowmapTexture = "Binary/Flowmap";
        }

        public static class DataPaths
        {
            public static readonly string CausticFolder = "CausticMaps";
            public static readonly string CausticDepthTexture = "KW_CausticDepthTexture";
            public static readonly string CausticDepthData = "KW_CausticDepthData";

            public static readonly string FlowmapFolder = "FlowMaps";
            public static readonly string FlowmapTexture = "FlowMapTexture";
            public static readonly string FluidsMaskTexture = "FluidsMaskTexture";
            public static readonly string FluidsPrebakedTexture = "FluidsPrebakedTexture";
            public static readonly string FlowmapData = "FlowMapData";

            public static readonly string SplineFolder = "Splines";
            public static readonly string SplineData = "SplineData";
        }

        public static class ShaderPaths
        {
            public static readonly string KWS_PlatformSpecificHelpers = @"Resources/PlatformSpecific/KWS_PlatformSpecificHelpers.cginc";
        }

        public static class FFT
        {
            public static readonly float MaxWindSpeed = 50;
            public static readonly float MaxWavesAreaScale = 4;

            public static readonly float[] FftDomainSizes = { 5, 20, 100, 600 };
            public static readonly float[] FftDomainVisiableArea = { 40, 160, 800, 4800 };
            public static readonly Vector4[] FftDomainScales =
            {
                new Vector4(1.0f,  0.4f,  1.0f,  0),
                new Vector4(0.95f, 0.4f,  0.95f, 0),
                new Vector4(0.95f, 0.45f, 0.95f, 0),
                new Vector4(0.9f,  0.5f,  0.9f,  0)
            };

            public static readonly int MaxLods = 4;
            public static readonly int MaxNormalsAnisoLevel = 16;
        }

        public static class Flowing
        {
            public static readonly float AreaSizeMultiplierLod1 = 3;
        }

        public static class Caustic
        {
            public static readonly float CausticDecalHeight = 100;
            public static readonly Vector4 LodSettings = new Vector4(10, 20, 40, 80);

            public static readonly float MaxWindDepthScaleMultiplier = 0.5f;
            public static readonly float MaxCausticDepth = 10;
        }

        public static class SurfaceDepth
        {
            public static readonly float MaxSurfaceDepthMeters = 50;
        }

        public static class Shoreline
        {
            public static readonly int ShorelineWavesTextureResolution = 2048;
            public static readonly float ShorelineWavesAreaSize = 100;

            public static readonly int[] LodDistances = { 20, 40, 60, 80, 100, 120, 140, 160, 180, 200 };

            public static readonly Dictionary<ShorelineFoamQualityEnum, float> LodOffset = new Dictionary<ShorelineFoamQualityEnum, float>
            {
                {
                    ShorelineFoamQualityEnum.High, +5
                },
                {
                    ShorelineFoamQualityEnum.Medium, 0
                },
                {
                    ShorelineFoamQualityEnum.Low, -5
                },
                {
                    ShorelineFoamQualityEnum.VeryLow, -10
                }
            };

            public static readonly Dictionary<ShorelineFoamQualityEnum, float> LodParticlesMultiplier = new Dictionary<ShorelineFoamQualityEnum, float>
            {
                {
                    ShorelineFoamQualityEnum.High, 1.35f
                },
                {
                    ShorelineFoamQualityEnum.Medium, 1.45f
                },
                {
                    ShorelineFoamQualityEnum.Low, 1.55f
                },
                {
                    ShorelineFoamQualityEnum.VeryLow, 1.75f
                }
            };

        }

        public static class VolumetricLighting
        {
            public static readonly float AbsorbtionOverrideMultiplier = 1;
            public static readonly int  MaxIterations        = 8;
        }

        public static class Reflection
        {
            public static readonly float MaxSunStrength               = 3;
            public static readonly bool  IsCloudRenderingAvailable    = false;
            public static readonly bool  IsVolumetricsAndFogAvailable = false;
            public static readonly float MaxSkyLodAtFarDistance       = 1.5f;
            public static readonly bool  IsReflectionProbeAvailable   = true;
        }
        public static class Refraction
        {
            public static readonly bool IsRefractionDownsampleAvailable = true;
        }

        public static class DynamicWaves
        {
            public static readonly int MaxDynamicWavesTexSize = 2048;
        }

        public static class Mesh
        {
            public static readonly int SplineRiverMinVertexCount = 5;
            public static readonly int SplineRiverMaxVertexCount = 25;

            public static readonly float MaxTesselationFactorInfinite = 12;
            public static readonly float MaxTesselationFactorFinite = 5;
            public static readonly float MaxTesselationFactorRiver = 5;
            public static readonly float MaxTesselationFactorCustom = 15;

            public static readonly int TesselationInfiniteMeshChunksSize = 2;
            public static readonly int TesselationFiniteMeshChunksSize = 2;

            public static readonly float MaxInfiniteOceanDepth = 5000;


            public static readonly float QuadtreeInfiniteOceanMinDistance = 10.0f;
            public static readonly float QuadtreeFiniteOceanMinDistance = 20.0f;
            public static readonly float UpdateQuadtreeEveryMetersForward = 5f;
            public static readonly float UpdateQuadtreeEveryMetersBackward = 1.0f;
            public static readonly float QuadtreeRotationThresholdForUpdate = 0.005f;
            public static readonly float QuadTreeAmplitudeDisplacementMultiplier = 1.25f;

            public static readonly Vector3 MinFiniteSize = new Vector3(0.25f, 0.25f, 0.25f);
        }
    }
}
