using System;
using UnityEngine;
using static KWS.WaterSystem;

namespace KWS
{
    public class WaterSystemScriptableData : ScriptableObject
    {
        //Color settings
        public float Transparent = 7;
        public Color DyeColor = new Color(1, 1, 1, 1); 
        public Color TurbidityColor = new Color(7 / 255.0f, 65 / 255.0f, 80 / 255.0f);

        //Waves settings
        public bool UseGlobalWind = true;
        public WindZone GlobalWindZone;
        public float GlobalWindZoneSpeedMultiplier = 1;
        public float GlobalWindZoneTurbulenceMultiplier = 1;
        public float GlobalWindSpeed = 5.0f;
        public float GlobalWindRotation = 0;
        public float GlobalWindTurbulence = 0.5f;
        public FftWavesQualityEnum GlobalFftWavesQuality = FftWavesQualityEnum.Ultra;
        public int GlobalFftWavesCascades = 4;
        public float GlobalWavesAreaScale = 1;
        public float GlobalTimeScale = 1;

        public float LocalWindSpeed      = 3.5f;
        public float LocalWindRotation = 0;
        public float LocalWindTurbulence = 0.4f;
        public FftWavesQualityEnum LocalFftWavesQuality = FftWavesQualityEnum.High;
        public int LocalFftWavesCascades = 4;
        public float LocalWavesAreaScale = 1;
        public float LocalTimeScale = 1;


        //Foam Settings
        public WaterProfileEnum FoamProfile          = WaterProfileEnum.High;
        public bool             UseOceanFoam         = false;
        public Color            OceanFoamColor       = new Color(0.535f, 0.649f, 0.664f, 0.86f);
        public float            OceanFoamStrength    = 0.2f;
        public float            OceanFoamTextureSize = 30;

        public bool  UseIntersectionFoam          = false;
        public Color IntersectionFoamColor        = new Color(165 / 255.0f, 230 / 255.0f, 255 / 255.0f, 235 / 255.0f);
        public float IntersectionFoamFadeDistance = 1.5f;
        public float IntersectionTextureFoamSize  = 25;


        //Reflection settings
        public WaterProfileEnum                           ReflectionProfile                      = WaterProfileEnum.High;
        public bool                                       UseScreenSpaceReflection               = true;
        public ScreenSpaceReflectionResolutionQualityEnum ScreenSpaceReflectionResolutionQuality = ScreenSpaceReflectionResolutionQualityEnum.High;
        public bool                                       UseScreenSpaceReflectionHolesFilling   = true;
        public bool                                       UseScreenSpaceReflectionSky  = true;
        public float                                      ScreenSpaceBordersStretching           = 0.025f;

        public bool                                  UsePlanarReflection               = false;
        public int                                   PlanarCullingMask                 = ~0;
        public PlanarReflectionResolutionQualityEnum PlanarReflectionResolutionQuality = PlanarReflectionResolutionQualityEnum.Medium;
        public float                                 ReflectionClipPlaneOffset         = 0.0005f;
        public bool                                  RenderPlanarShadows               = false;
        public bool                                  RenderPlanarVolumetricsAndFog     = false;
        public bool                                  RenderPlanarClouds                = false;

        public bool UseReflectionProbes = false;

        public bool OverrideSkyColor = false;
        public Color CustomSkyColor = Color.gray;

        public bool UseAnisotropicReflections = true;
        public bool AnisotropicReflectionsHighQuality = false;
        public float AnisotropicReflectionsScale = 0.5f;

        public bool ReflectSun = true;
        public float ReflectedSunCloudinessStrength = 0.04f;
        public float ReflectedSunStrength = 1.0f;



        //Refraction settings
        public WaterProfileEnum         RefractionProfile            = WaterProfileEnum.High;
        public RefractionModeEnum       RefractionMode               = RefractionModeEnum.PhysicalAproximationIOR;
        public float                    RefractionAproximatedDepth   = 2f;
        public float                    RefractionSimpleStrength     = 0.25f;
        public bool                     UseRefractionDispersion      = true;
        public float                    RefractionDispersionStrength = 0.35f;
        public RefractionResolutionEnum RefractionResolution         = RefractionResolutionEnum.Half;


        //Volumetric settings
        public WaterProfileEnum                     VolumetricLightProfile           = WaterProfileEnum.High;
        public bool                                 UseVolumetricLight               = true;
        public VolumetricLightResolutionQualityEnum VolumetricLightResolutionQuality = VolumetricLightResolutionQualityEnum.High;
        public int                                  VolumetricLightIteration         = 8;
        public float VolumetricLightTemporalReprojectionAccumulationFactor = 0.35f;

        //public float VolumetricLightDirLightIntensityMultiplier = 1;
        //public float VolumetricLightAdditionalLightsIntensityMultiplier = 1;

        //public float VolumetricLightCausticStrength            = 1f;
        public bool  VolumetricLightUseAdditionalLightsCaustic = false;

        //FlowMap settings
        public WaterProfileEnum FlowmapProfile = WaterProfileEnum.High;
        public bool UseFlowMap = false;
        public Vector3 FlowMapAreaPosition = new Vector3(0, 0, 0);
        public int FlowMapAreaSize = 200;
        public FlowmapTextureResolutionEnum FlowMapTextureResolution = FlowmapTextureResolutionEnum._2048;
        public float FlowMapSpeed = 1;
        public bool UseFluidsSimulation = false;
        public int FluidsAreaSize = 40;
        public int FluidsSimulationIterrations = 2;
        public int FluidsTextureSize = 1024;
        public int FluidsSimulationFPS = 60;
        public float FluidsSpeed = 1;
        public float FluidsFoamStrength = 0.5f;
        public FlowingScriptableData FlowingScriptableData;


        //Dynamic waves settings
        public WaterProfileEnum DynamicWavesProfile = WaterProfileEnum.High;
        public bool UseDynamicWaves = false;
        public int DynamicWavesAreaSize = 25;
        public int DynamicWavesResolutionPerMeter = 40;
        public float DynamicWavesGlobalForceScale = 0.2f;
        public float DynamicWavesPropagationSpeed = 1.0f;
        public bool UseDynamicWavesRainEffect;
        public float DynamicWavesRainStrength = 0.2f;



        //Shoreline settings
        public WaterProfileEnum         ShorelineProfile               = WaterProfileEnum.High;
        public bool                     UseShorelineRendering          = false;
        public Color                    ShorelineColor                 = new Color(292 / 255.0f, 310 /255.0f, 315 / 255.0f, 160 /255.0f);
        public ShorelineFoamQualityEnum ShorelineFoamLodQuality        = ShorelineFoamQualityEnum.High;
        public bool                     UseShorelineFoamFastMode       = false;
        public bool                     ShorelineFoamReceiveDirShadows = true;
        //public bool FoamReceiveAdditionalLightsShadows = false;
        public ShorelineWavesScriptableData ShorelineWavesScriptableData;

        //Caustic settings
        public WaterProfileEnum                    CausticProfile                        = WaterProfileEnum.High;
        public bool                                UseCausticEffect                      = true;

        public bool                                UseGlobalCausticHighQualityFiltering  = false;
        public CausticTextureResolutionQualityEnum GlobalCausticTextureResolutionQuality = CausticTextureResolutionQualityEnum.High;
        public float                               GlobalCausticDepth                          = 2;

        public bool                                UseLocalCausticHighQualityFiltering  = false;
        public CausticTextureResolutionQualityEnum LocalCausticTextureResolutionQuality = CausticTextureResolutionQualityEnum.High;
        public float                               LocalCausticDepth                    = 2;

        public bool                                UseCausticDispersion                 = true;
        public float                               CausticStrength                      = 2;


        //Underwater settings
        public WaterProfileEnum             UnderwaterProfile                  = WaterProfileEnum.High;
        public bool                         UseUnderwaterEffect                = true;
        public UnderwaterReflectionModeEnum UnderwaterReflectionMode           = UnderwaterReflectionModeEnum.PhysicalAproximatedReflection;
        public bool                         UseUnderwaterHalfLineTensionEffect = true;
        public float                        UnderwaterHalfLineTensionScale     = 0.5f;
        public bool                         OverrideUnderwaterTransparent      = false;
        public float                        UnderwaterTransparentOffset        = 5;

        //Mesh settings
        public WaterProfileEnum MeshProfile = WaterProfileEnum.High;
        public WaterMeshTypeEnum WaterMeshType;
        public bool                 UseAquariumMode                     = false;
        public int                  OceanDetailingFarDistance           = 1000;
        public float                RiverSplineNormalOffset             = 1;
        public int                  RiverSplineVertexCountBetweenPoints = 20;
        public int                  RiverSplineDepth                    = 10;
        public Mesh                 CustomMesh;
        public WaterMeshQualityEnum WaterMeshQualityInfinite = WaterMeshQualityEnum.High;
        public WaterMeshQualityEnum WaterMeshQualityFinite   = WaterMeshQualityEnum.High;
        //public Vector3 MeshSize = new Vector3(10, 10, 10);
        
        public bool UseTesselation = false;

        public float TesselationFactor_Infinite = 0.5f;
        public float TesselationFactor_Finite = 0.5f;
        public float TesselationFactor_Custom = 0.5f;
        public float TesselationFactor_River = 0.5f;

        public float TesselationMaxDistance_Infinite = 500f;
        public float TesselationMaxDistance_Finite = 100f;
        public float TesselationMaxDistance_Custom = 100f;
        public float TesselationMaxDistance_River = 100f;
        public SplineScriptableData SplineScriptableData;


        //Rendering settings
        public WaterProfileEnum RenderingProfile     = WaterProfileEnum.High;
        public int              TransparentSortingPriority           = -1;
        //public bool             EnabledMeshRendering = true;
        public bool             DrawToPosteffectsDepth;
        public bool             WireframeMode;

        #region public enums

        public enum WaterProfileEnum
        {
            Custom,
            Ultra,
            High,
            Medium,
            Low,
            PotatoPC
        }

        public enum WindSourceEnum
        {
            GlobalWind,
            OverrideWind,
            UnityWindZone
        }


        public enum FftWavesQualityEnum
        {
            Ultra   = 256,
            High    = 128,
            Medium  = 64,
            Low     = 32
        }

        public enum PlanarReflectionResolutionQualityEnum
        {
            Ultra = 768,
            High = 512,
            Medium = 368,
            Low = 256,
            VeryLow = 128
        }

        /// <summary>
        /// Resolution quality in percent relative to current screen size. For example Medium quality = 35, it's mean ScreenSize * (35 / 100)
        /// </summary>
        public enum ScreenSpaceReflectionResolutionQualityEnum
        {
            Ultra = 75,
            High = 50,
            Medium = 35,
            Low = 25,
            VeryLow = 20,
        }

        public enum CubemapReflectionResolutionQualityEnum
        {
            High = 512,
            Medium = 256,
            Low = 128,
        }


        public enum RefractionModeEnum
        {
            Simple,
            PhysicalAproximationIOR
        }

        public enum RefractionResolutionEnum
        {
            Full = 100,
            Half = 50,
            Quarter = 25
        }

        public enum UnderwaterReflectionModeEnum
        {
            NoReflection,
            PhysicalAproximatedReflection
        }

        public enum FoamShadowMode
        {
            None,
            CastOnly,
            CastAndReceive
        }

        public enum WaterMeshTypeEnum
        {
            InfiniteOcean = 0,
            FiniteBox = 1,
            River = 2,
            CustomMesh = 3
        }


        public enum WaterMeshQualityEnum
        {
            Ultra,
            High,
            Medium,
            Low,
            VeryLow,
        }


        public enum VolumetricLightFilterEnum
        {
            Bilateral,
            Gaussian
        }

        public enum VolumetricLightResolutionQualityEnum
        {
            Ultra = 75,
            High = 50,
            Medium = 35,
            Low = 25,
            VeryLow = 15,
        }

        public enum FlowmapTextureResolutionEnum
        {
            _512 = 512,
            _1024 = 1024,
            _2048 = 2048,
            _4096 = 4096,
        }

        public enum CausticTextureResolutionQualityEnum
        {
            Ultra = 1536,
            High = 1024,
            Medium = 768,
            Low = 384,
        }

        public enum ShorelineFoamQualityEnum
        {
            High,
            Medium,
            Low,
            VeryLow,
        }


        #endregion


        public float CurrentWindSpeed
        {
            get
            {
                if (UseGlobalWind)
                {
                    return GlobalWindZone == null ? GlobalWindSpeed : Mathf.Min(KWS_Settings.FFT.MaxWindSpeed, GlobalWindZone.windMain * GlobalWindZoneSpeedMultiplier);
                }
                else return LocalWindSpeed;
            }
        }

        public float CurrentWindRotation
        {
            get
            {
                if (UseGlobalWind)
                {
                    if (GlobalWindZone == null) return GlobalWindRotation * Mathf.Deg2Rad;
                    else
                    {
                        var forwardVec = GlobalWindZone.transform.forward;
                        return Mathf.Atan2(forwardVec.z, forwardVec.x);
                    }
                }
                else return LocalWindRotation * Mathf.Deg2Rad;
            }
        }

        public float CurrentWindTurbulence
        {
            get
            {
                if (UseGlobalWind)
                {
                    return GlobalWindZone == null ? GlobalWindTurbulence : Mathf.Clamp(GlobalWindZone.windTurbulence * GlobalWindZoneTurbulenceMultiplier, -1f, 1f);
                }
                else return LocalWindTurbulence;
            }
        }
        public FftWavesQualityEnum CurrentFftWavesQuality => UseGlobalWind ? GlobalFftWavesQuality : LocalFftWavesQuality;
        public int CurrentFftWavesCascades => UseGlobalWind ? GlobalFftWavesCascades : LocalFftWavesCascades;
        public float CurrentWavesAreaScale => UseGlobalWind ? GlobalWavesAreaScale : LocalWavesAreaScale;
        public float CurrentTimeScale => UseGlobalWind ? GlobalTimeScale : LocalTimeScale;


        public bool                                GetCurrentCausticHighQualityFiltering     => UseGlobalWind ? UseGlobalCausticHighQualityFiltering : UseLocalCausticHighQualityFiltering;
        public CausticTextureResolutionQualityEnum GetCurrentCausticTextureResolutionQuality => UseGlobalWind ? GlobalCausticTextureResolutionQuality : LocalCausticTextureResolutionQuality;
        public float                               GetCurrentCausticDepth                    => UseGlobalWind ? GlobalCausticDepth : LocalCausticDepth;

        public float CurrentTesselationFactor 
        {
            get
            {
                return WaterMeshType switch
                {
                    WaterMeshTypeEnum.InfiniteOcean => TesselationFactor_Infinite * KWS_Settings.Mesh.MaxTesselationFactorInfinite,
                    WaterMeshTypeEnum.FiniteBox     => TesselationFactor_Finite * KWS_Settings.Mesh.MaxTesselationFactorFinite,
                    WaterMeshTypeEnum.River         => TesselationFactor_River * KWS_Settings.Mesh.MaxTesselationFactorRiver,
                    WaterMeshTypeEnum.CustomMesh    => TesselationFactor_Custom * KWS_Settings.Mesh.MaxTesselationFactorCustom,
                    _                               => 0.5f
                };
            }
        }

        public float CurrentTesselationMaxDistance
        {
            get
            {
                return WaterMeshType switch
                {
                    WaterMeshTypeEnum.InfiniteOcean => Mathf.Min(OceanDetailingFarDistance * 0.5f, TesselationMaxDistance_Infinite),
                    WaterMeshTypeEnum.FiniteBox     => TesselationMaxDistance_Finite,
                    WaterMeshTypeEnum.River         => TesselationMaxDistance_River,
                    WaterMeshTypeEnum.CustomMesh    => TesselationMaxDistance_Custom,
                    _                               => 100
                };
            }
        }

        public bool CanUseAquariumMode => UseAquariumMode && (WaterMeshType == WaterMeshTypeEnum.FiniteBox || WaterMeshType == WaterMeshTypeEnum.CustomMesh);

    }
}