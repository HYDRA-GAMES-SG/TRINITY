using UnityEngine;
using UnityEngine.Rendering;

namespace KWS
{
    internal static class KWS_ShaderConstants
    {
        #region Shader names

        public static class ShaderNames
        {
            public static readonly string WaterShaderName                      = "Hidden/KriptoFX/KWS/Water";
            public static readonly string WaterTesselatedShaderName            = "Hidden/KriptoFX/KWS/WaterTesselated";
            public static readonly string WaterPrePassShaderName               = "Hidden/KriptoFX/KWS/WaterPrePass";
            public static readonly string VolumetricLightingShaderName         = "Hidden/KriptoFX/KWS/VolumetricLighting";
            public static readonly string BlurBilateralName                    = "Hidden/KriptoFX/KWS/BlurBilateral";
            public static readonly string ReflectionFiltering                  = "Hidden/KriptoFX/KWS/AnisotropicFiltering";
            public static readonly string CausticComputeShaderName             = "Hidden/KriptoFX/KWS/Caustic_Pass";
            public static readonly string CausticDecalShaderName               = "Hidden/KriptoFX/KWS/CausticDecal";
            public static readonly string FlowMapShaderName                    = "Hidden/KriptoFX/KWS/FlowMapEdit";
            public static readonly string UnderwaterShaderName                 = "Hidden/KriptoFX/KWS/Underwater";
            public static readonly string UnderwaterBlurToScreenShaderName     = "Hidden/KriptoFX/KWS/UnderwaterBlurToScreenPass";
            public static readonly string CopyColorShaderName                  = "Hidden/KriptoFX/KWS/CopyColorTexture";
            public static readonly string DrawToDepthShaderName                = "Hidden/KriptoFX/KWS/DrawToDepth";
            public static readonly string CopyDepthShaderName                  = "Hidden/KriptoFX/KWS/CopyDepthTexture";
            public static readonly string CombineSeparatedTexturesToArrayVR    = "Hidden/KriptoFX/KWS/KWS_CombineSeparatedTexturesToArrayVR";
            public static readonly string ShorelineBakedWavesShaderName        = "Hidden/KriptoFX/KWS/ShorelineWaves";
            public static readonly string ShorelineFoamDrawToScreenName        = "Hidden/KriptoFX/KWS/ShorelineFoamDrawToScreen";
            public static readonly string InfiniteOceanShadowCullingAnchorName = "Hidden/KriptoFX/KWS/ShadowFix";

            public static readonly string DynamicWavesShaderName = "Hidden/KriptoFX/KWS/DynamicWaves";

            public static readonly string SsrComputePath                      = "PlatformSpecific/KWS_ScreenSpaceReflectionCompute";
            public static readonly string ClipMaskShaderName                  = "Hidden/KriptoFX/KWS/ClipMask";
            public static readonly string SsrShaderName                       = "Hidden/KriptoFX/KWS/SSR";
            public static readonly string TileBasedReflectionProbesComputeShaderName = "Common/CommandPass/KWS_TileBasedReflectionProbes";
        }

        #endregion

        #region ShaderID

        public static class CameraMatrix
        {
            public static readonly int KWS_MATRIX_VP                       = Shader.PropertyToID("KWS_MATRIX_VP");
            public static readonly int KWS_PREV_MATRIX_VP                  = Shader.PropertyToID("KWS_PREV_MATRIX_VP");
            public static readonly int KWS_MATRIX_I_VP                     = Shader.PropertyToID("KWS_MATRIX_I_VP");
            public static readonly int KWS_MATRIX_CAMERA_PROJECTION_STEREO = Shader.PropertyToID("KWS_MATRIX_CAMERA_PROJECTION_STEREO");
            public static readonly int KWS_MATRIX_VP_STEREO                = Shader.PropertyToID("KWS_MATRIX_VP_STEREO");
            public static readonly int KWS_PREV_MATRIX_VP_STEREO           = Shader.PropertyToID("KWS_PREV_MATRIX_VP_STEREO");
            public static readonly int KWS_MATRIX_I_VP_STEREO              = Shader.PropertyToID("KWS_MATRIX_I_VP_STEREO");
            public static readonly int KWS_MATRIX_CAMERA_PROJECTION        = Shader.PropertyToID("KWS_MATRIX_CAMERA_PROJECTION");
            public static readonly int KWS_MATRIX_VP_ORTHO                 = Shader.PropertyToID("KWS_MATRIX_VP_ORTHO");

            public static readonly int KWS_MATRIX_I_P        = Shader.PropertyToID("KWS_MATRIX_I_P");
            public static readonly int KWS_MATRIX_I_P_STEREO = Shader.PropertyToID("KWS_MATRIX_I_P_STEREO");

            public static readonly int KW_ViewToWorld = Shader.PropertyToID("KW_ViewToWorld");
            public static readonly int KW_ProjToView = Shader.PropertyToID("KW_ProjToView");


        }


        public static class ArrayWaterParams
        {
            public static readonly int KWS_WaterInstancesCount = Shader.PropertyToID("KWS_WaterInstancesCount");
            public static readonly int KWS_WindSpeedArray = Shader.PropertyToID("KWS_WindSpeedArray");

            public static readonly int KWS_TransparentArray           = Shader.PropertyToID("KWS_TransparentArray");
            public static readonly int KWS_UnderwaterTransparentOffsetArray = Shader.PropertyToID("KWS_UnderwaterTransparentOffsetArray");
            public static readonly int KWS_DyeColorArray              = Shader.PropertyToID("KWS_DyeColorArray");
            public static readonly int KWS_TurbidityColorArray        = Shader.PropertyToID("KWS_TurbidityColorArray");

            public static readonly int KWS_WaterPositionArray   = Shader.PropertyToID("KWS_WaterPositionArray");
            public static readonly int KWS_CausticStrengthArray = Shader.PropertyToID("KWS_CausticStrengthArray");
            public static readonly int KWS_WavesAreaScaleArray  = Shader.PropertyToID("KWS_WavesAreaScaleArray");
            public static readonly int KWS_MeshTypeArray        = Shader.PropertyToID("KWS_MeshTypeArray");

            public static readonly int KWS_ActiveSsrInstances = Shader.PropertyToID("KWS_ActiveSsrInstances");
        }

        public static class ConstantWaterParams
        {
            public static readonly int KWS_WaterLayerMask   = Shader.PropertyToID("KWS_WaterLayerMask");
            public static readonly int KWS_WaterLightLayerMask   = Shader.PropertyToID("KWS_WaterLightLayerMask");
            public static readonly int KWS_StencilMaskValue = Shader.PropertyToID("KWS_StencilMaskValue");

            public static readonly int KW_Transparent    = Shader.PropertyToID("KW_Transparent");
            public static readonly int KWS_DyeColor      = Shader.PropertyToID("KWS_DyeColor");
            public static readonly int KW_TurbidityColor = Shader.PropertyToID("KW_TurbidityColor");

            public static readonly int KWS_WindSpeed = Shader.PropertyToID("KWS_WindSpeed");
            public static readonly int KWS_WindRotation = Shader.PropertyToID("KWS_WindRotation");
            public static readonly int KWS_WindTurbulence = Shader.PropertyToID("KWS_WindTurbulence");
            public static readonly int KWS_WavesAreaScale = Shader.PropertyToID("KWS_WavesAreaScale");
            public static readonly int KWS_WavesCascades = Shader.PropertyToID("KWS_WavesCascades");
            public static readonly int KWS_WavesDomainSizes = Shader.PropertyToID("KWS_WavesDomainSizes");
            public static readonly int KWS_WavesDomainScales = Shader.PropertyToID("KWS_WavesDomainScales");
            public static readonly int KWS_WavesDomainVisiableArea = Shader.PropertyToID("KWS_WavesDomainVisiableArea");
            public static readonly int KW_GlobalTimeScale = Shader.PropertyToID("KW_GlobalTimeScale");

            public static readonly int KWS_ScreenSpaceBordersStretching = Shader.PropertyToID("KWS_ScreenSpaceBordersStretching");
            public static readonly int UseScreenSpaceReflectionSky = Shader.PropertyToID("UseScreenSpaceReflectionSky");
            public static readonly int KWS_AnisoReflectionsScale = Shader.PropertyToID("KWS_AnisoReflectionsScale");
            public static readonly int KWS_SkyLodRelativeToWind = Shader.PropertyToID("KWS_SkyLodRelativeToWind");
            public static readonly int KW_ReflectionClipOffset = Shader.PropertyToID("KWS_ReflectionClipOffset");
            public static readonly int KWS_OverrideSkyColor = Shader.PropertyToID("KWS_OverrideSkyColor");
            public static readonly int KWS_CustomSkyColor = Shader.PropertyToID("KWS_CustomSkyColor");
            public static readonly int KWS_SunCloudiness = Shader.PropertyToID("KWS_SunCloudiness");
            public static readonly int KWS_SunStrength = Shader.PropertyToID("KWS_SunStrength");
            public static readonly int KWS_SunMaxValue = Shader.PropertyToID("KWS_SunMaxValue");

            public static readonly int KWS_RefractionAproximatedDepth = Shader.PropertyToID("KWS_RefractionAproximatedDepth");
            public static readonly int KWS_RefractionSimpleStrength = Shader.PropertyToID("KWS_RefractionSimpleStrength");
            public static readonly int KWS_RefractionDispersionStrength = Shader.PropertyToID("KWS_RefractionDispersionStrength");

            public static readonly int KW_FlowMapOffset = Shader.PropertyToID("KW_FlowMapOffset");
            public static readonly int KW_FlowMapSize = Shader.PropertyToID("KW_FlowMapSize");
            public static readonly int KW_FlowMapSpeed = Shader.PropertyToID("KW_FlowMapSpeed");
            public static readonly int KW_FlowMapFluidsStrength = Shader.PropertyToID("KW_FlowMapFluidsStrength");

            public static readonly int KWS_UnderwaterHalfLineTensionScale = Shader.PropertyToID("KWS_UnderwaterHalfLineTensionScale");

            public static readonly int _TesselationMaxDisplace = Shader.PropertyToID("_TesselationMaxDisplace");
            public static readonly int _TesselationFactor = Shader.PropertyToID("_TesselationFactor");
            public static readonly int _TesselationMaxDistance = Shader.PropertyToID("_TesselationMaxDistance");
            public static readonly int KWS_MeshType = Shader.PropertyToID("KWS_MeshType");
            public static readonly int KWS_WaterYRotationRad = Shader.PropertyToID("KWS_WaterYRotationRad");
            public static readonly int KWS_InstancingWaterScale = Shader.PropertyToID("KWS_InstancingWaterScale");
            public static readonly int KW_WaterFarDistance = Shader.PropertyToID("KW_WaterFarDistance");

            public static readonly int KWS_OceanFoamStrengthSize = Shader.PropertyToID("KWS_OceanFoamStrengthSize");
            public static readonly int KWS_OceanFoamColor = Shader.PropertyToID("KWS_OceanFoamColor");

            public static readonly int KWS_IntersectionFoamColor = Shader.PropertyToID("KWS_IntersectionFoamColor");
            public static readonly int KWS_IntersectionFoamFadeDistance = Shader.PropertyToID("KWS_IntersectionFoamFadeDistance");
            public static readonly int KWS_IntersectionFoamFadeSize = Shader.PropertyToID("KWS_IntersectionFoamFadeSize");

            public static readonly int KWS_UseOceanFoam            = Shader.PropertyToID("KWS_UseOceanFoam");
            public static readonly int KWS_UseIntersectionFoam     = Shader.PropertyToID("KWS_UseIntersectionFoam");
            public static readonly int KWS_UseWireframeMode        = Shader.PropertyToID("KWS_UseWireframeMode");
            public static readonly int KWS_UseMultipleSimulations  = Shader.PropertyToID("KWS_UseMultipleSimulations");
            public static readonly int KWS_UseRefractionIOR        = Shader.PropertyToID("KWS_UseRefractionIOR");
            public static readonly int KWS_UseRefractionDispersion = Shader.PropertyToID("KWS_UseRefractionDispersion");
            public static readonly int KWS_UseFilteredNormals      = Shader.PropertyToID("KWS_UseFilteredNormals");

            public static readonly int KWS_AbsorbtionOverrideMultiplier = Shader.PropertyToID("KWS_AbsorbtionOverrideMultiplier");
        }

        public static class DynamicWaterParams
        {
            public static readonly int KW_WaterPosition = Shader.PropertyToID("KW_WaterPosition");
            public static readonly int KW_Time = Shader.PropertyToID("KW_Time");
            public static readonly int KWS_ScaledTime = Shader.PropertyToID("KWS_ScaledTime");
            public static readonly int KWS_AmbientColor = Shader.PropertyToID("KWS_AmbientColor");
            public static readonly int KWS_WaterInstanceID = Shader.PropertyToID("KWS_WaterInstanceID");
            public static readonly int KWS_CameraForward = Shader.PropertyToID("KWS_CameraForward");
            public static readonly int KWS_UnderwaterVisible = Shader.PropertyToID("KWS_UnderwaterVisible");
            public static readonly int KWS_FogState = Shader.PropertyToID("KWS_FogState");
            public static readonly int KWS_DirLightDireciton = Shader.PropertyToID("KWS_DirLightDireciton");
            public static readonly int KWS_DirLightColor = Shader.PropertyToID("KWS_DirLightColor");
            //public static readonly int KWS_UseClipMask = Shader.PropertyToID("KWS_UseClipMask");
        }

        public static class PrePass
        {
            public static readonly int KWS_WaterVolumeDataBufferLength = Shader.PropertyToID("KWS_WaterVolumeDataBufferLength");
            public static readonly int KWS_WaterVolumeDataBuffer       = Shader.PropertyToID("KWS_WaterVolumeDataBuffer");
            public static readonly int KWS_OceanLevel = Shader.PropertyToID("KWS_OceanLevel");
        }

        public static class FFT
        {
            public static readonly int[] KW_DispTex =
            {
                Shader.PropertyToID("KW_DispTex"),
                Shader.PropertyToID("KW_DispTex_LOD1"),
                Shader.PropertyToID("KW_DispTex_LOD2")
            };

            public static readonly int[] KW_NormTex =
            {
                Shader.PropertyToID("KW_NormTex"),
                Shader.PropertyToID("KW_NormTex_LOD1"),
                Shader.PropertyToID("KW_NormTex_LOD2")
            };

            public static readonly int[] KW_FFTDomainSize =
            {
                Shader.PropertyToID("KW_FFTDomainSize"),
                Shader.PropertyToID("KW_FFTDomainSize_LOD1"),
                Shader.PropertyToID("KW_FFTDomainSize_LOD2")
            };

            public static readonly int KWS_FftWavesDisplace = Shader.PropertyToID("KWS_FftWavesDisplace");
            public static readonly int KWS_FftWavesDisplacePrev = Shader.PropertyToID("KWS_FftWavesDisplacePrev");
            public static readonly int KWS_FftWavesNormal = Shader.PropertyToID("KWS_FftWavesNormal");
        }

        public static class OrthoDepth
        {
            public static readonly int KWS_OrthoDepthPos = Shader.PropertyToID("KWS_OrthoDepthPos");
            public static readonly int KWS_OrthoDepthNearFarSize = Shader.PropertyToID("KWS_OrthoDepthNearFarSize");
            public static readonly int KWS_WaterOrthoDepthRT = Shader.PropertyToID("KWS_WaterOrthoDepthRT");
        }

        public static class MaskPassID
        {
            public static readonly int KWS_WaterPrePassRT0                        = Shader.PropertyToID("KWS_WaterPrePassRT0");
            public static readonly int KWS_WaterPrePassRT1                        = Shader.PropertyToID("KWS_WaterPrePassRT1");
            public static readonly int KWS_WaterDepthRT                           = Shader.PropertyToID("KWS_WaterDepthRT");
            public static readonly int KWS_WaterIntersectionHalfLineTensionMaskRT = Shader.PropertyToID("KWS_WaterIntersectionHalfLineTensionMaskRT");

            public static readonly int KWS_WaterAquariumPrePassRT0 = Shader.PropertyToID("KWS_WaterAquariumPrePassRT0");
            public static readonly int KWS_WaterAquariumDepthRT            = Shader.PropertyToID("KWS_WaterAquariumDepthRT");

            public static readonly int KWS_WaterBackfacePrePassRT0 = Shader.PropertyToID("KWS_WaterBackfacePrePassRT0");
            public static readonly int KWS_WaterBackfacePrePassRT1 = Shader.PropertyToID("KWS_WaterBackfacePrePassRT1");
            public static readonly int KWS_WaterBackfaceDepthRT    = Shader.PropertyToID("KWS_WaterBackfaceDepthRT");

            public static readonly int KWS_WaterPrePass_RTHandleScale                    = Shader.PropertyToID("KWS_WaterPrePass_RTHandleScale");
            public static readonly int KWS_WaterBackfacePrePass_RTHandleScale = Shader.PropertyToID("KWS_WaterBackfacePrePass_RTHandleScale");
        }

        public static class ReflectionsID
        {
            //public static readonly int KWS_ScreenSpaceBordersStretching = Shader.PropertyToID("KWS_ScreenSpaceBordersStretching");
            //public static readonly int KWS_IgnoreAnisotropicScreenSpaceSky = Shader.PropertyToID("KWS_IgnoreAnisotropicScreenSpaceSky");
            //public static readonly int KWS_AnisoReflectionsScale = Shader.PropertyToID("KWS_AnisoReflectionsScale");
            //public static readonly int KWS_NormalizedWind = Shader.PropertyToID("KWS_NormalizedWind");

            public static readonly int KWS_PlanarReflectionRT = Shader.PropertyToID("KWS_PlanarReflectionRT");
            public static readonly int KWS_PlanarReflectionInstanceID = Shader.PropertyToID("KWS_PlanarReflectionInstanceID");

            public static readonly int KWS_SkyTexture = Shader.PropertyToID("KWS_SkyTexture");
            public static readonly int KWS_SkyTexture_HDRDecodeValues = Shader.PropertyToID("KWS_SkyTexture_HDRDecodeValues");
            public static readonly int KWS_IsCubemapReflectionPlanar = Shader.PropertyToID("KWS_IsCubemapReflectionPlanar");

            public static readonly int KWS_ReprojectedFrameReady = Shader.PropertyToID("KWS_ReprojectedFrameReady");
        }

        public static class RefractionID
        {

        }

        public static class SSR_ID
        {
            public static readonly int _RTSize = Shader.PropertyToID("_RTSize");
            public static readonly int KWS_AverageInstancesHeight = Shader.PropertyToID("KWS_AverageInstancesHeight");

            public static readonly int HashRT = Shader.PropertyToID("HashRT");
            public static readonly int ColorRT = Shader.PropertyToID("ColorRT");
            public static readonly int _CameraDepthTexture = Shader.PropertyToID("_CameraDepthTexture");
            public static readonly int _CameraOpaqueTexture = Shader.PropertyToID("_CameraOpaqueTexture");
            public static readonly int KWS_ScreenSpaceReflectionRT = Shader.PropertyToID("KWS_ScreenSpaceReflectionRT");
            public static readonly int KWS_ScreenSpaceReflection_RTHandleScale = Shader.PropertyToID("KWS_ScreenSpaceReflection_RTHandleScale");
            public static readonly int KWS_LastTargetRT = Shader.PropertyToID("KWS_LastTargetRT");
        }

        public static class VolumetricLightConstantsID
        {
            public static readonly int KWS_VolumetricLightRT_Last                    = Shader.PropertyToID("KWS_VolumetricLightRT_Last");
            public static readonly int KWS_VolumetricLightRT_Last_RTHandleScale      = Shader.PropertyToID("KWS_VolumetricLightRT_Last_RTHandleScale");
            public static readonly int KWS_Frame                                     = Shader.PropertyToID("KWS_Frame");
            public static readonly int KWS_ReprojectedFrameReady                     = Shader.PropertyToID("KWS_ReprojectedFrameReady");
            public static readonly int KWS_VolumetricLightTemporalAccumulationFactor = Shader.PropertyToID("KWS_VolumetricLightTemporalAccumulationFactor");
            public static readonly int KWS_VolumetricLightDownscaleFactor        = Shader.PropertyToID("KWS_VolumetricLightDownscaleFactor");


            public static readonly int KWS_CameraDepthTextureLowRes               = Shader.PropertyToID("KWS_CameraDepthTextureLowRes");
            public static readonly int KWS_CameraDepthTextureLowRes_RTHandleScale = Shader.PropertyToID("KWS_CameraDepthTextureLowRes_RTHandleScale");

            public static readonly int KWS_BlurDepthFactor                        = Shader.PropertyToID("KWS_BlurDepthFactor");

            public static readonly int KWS_VolumetricLightRT               = Shader.PropertyToID("KWS_VolumetricLightRT");
            public static readonly int KWS_VolumetricLightAdditionalDataRT = Shader.PropertyToID("KWS_VolumetricLightAdditionalDataRT");

            public static readonly int KWS_Transparent = Shader.PropertyToID("KWS_Transparent");
            public static readonly int KWS_LightAnisotropy = Shader.PropertyToID("KWS_LightAnisotropy");
            public static readonly int KWS_VolumeDepthFade = Shader.PropertyToID("KWS_VolumeDepthFade");
            public static readonly int KWS_VolumeLightMaxDistance = Shader.PropertyToID("KWS_VolumeLightMaxDistance");
            public static readonly int KWS_RayMarchSteps = Shader.PropertyToID("KWS_RayMarchSteps");
            public static readonly int KWS_VolumeLightBlurRadius = Shader.PropertyToID("KWS_VolumeLightBlurRadius");
            public static readonly int KWS_VolumeTexSceenSize = Shader.PropertyToID("KWS_VolumeTexSceenSize");
            public static readonly int KWS_VolumetricLight_RTHandleScale = Shader.PropertyToID("KWS_VolumetricLight_RTHandleScale");
            public static readonly int KWS_WorldSpaceCameraPosCompillerFixed = Shader.PropertyToID("KWS_WorldSpaceCameraPosCompillerFixed");

        }
        public static class CausticID
        {
            public static readonly int KWS_CausticRT = Shader.PropertyToID("KWS_CausticRT");
            public static readonly int KWS_CausticRTArray = Shader.PropertyToID("KWS_CausticRTArray");
            public static readonly int KWS_InstanceToCausticID = Shader.PropertyToID("KWS_InstanceToCausticID");
            public static readonly int KWS_InstanceToCausticViewportScale = Shader.PropertyToID("KWS_InstanceToCausticViewportScale");
            public static readonly int KWS_CausticDepthScale = Shader.PropertyToID("KWS_CausticDepthScale");
            public static readonly int KWS_CaustisStrength = Shader.PropertyToID("KWS_CaustisStrength");
            public static readonly int KWS_CaustisDispersionStrength = Shader.PropertyToID("KWS_CaustisDispersionStrength");
            //public static readonly int KW_CausticDispersionStrength = Shader.PropertyToID("KW_CausticDispersionStrength");
            //public static readonly int KW_MeshScale = Shader.PropertyToID("KW_MeshScale");

            //public static readonly int KW_CausticCameraOffset = Shader.PropertyToID("KW_CausticCameraOffset");

            //public static readonly int[] KW_CausticLod =
            //{
            //    Shader.PropertyToID("KW_CausticLod0"),
            //    Shader.PropertyToID("KW_CausticLod1"),
            //    Shader.PropertyToID("KW_CausticLod2"),
            //    Shader.PropertyToID("KW_CausticLod3")
            //};

            //public static readonly int KW_CausticLodSettings = Shader.PropertyToID("KW_CausticLodSettings");
            //public static readonly int KW_CausticLodOffset = Shader.PropertyToID("KW_CausticLodOffset");
            //public static readonly int KW_CausticLodPosition = Shader.PropertyToID("KW_CausticLodPosition");
            //public static readonly int KW_DecalScale = Shader.PropertyToID("KW_DecalScale");


            //public static readonly int KW_CausticDepthTex = Shader.PropertyToID("KW_CausticDepthTex");
            //public static readonly int KW_CausticDepthOrthoSize = Shader.PropertyToID("KW_CausticDepthOrthoSize");
            //public static readonly int KW_CausticDepth_Near_Far_Dist = Shader.PropertyToID("KW_CausticDepth_Near_Far_Dist");
            //public static readonly int KW_CausticDepthPos = Shader.PropertyToID("KW_CausticDepthPos");
        }

        public static class FlowmapID
        {
            public static readonly int KW_FlowMapTex = Shader.PropertyToID("KW_FlowMapTex");

            public static readonly int KW_FluidsVelocityAreaScale       = Shader.PropertyToID("KW_FluidsVelocityAreaScale");
            public static readonly int KW_FluidsMapWorldPosition_lod0   = Shader.PropertyToID("KW_FluidsMapWorldPosition_lod0");
            public static readonly int KW_FluidsMapWorldPosition_lod1   = Shader.PropertyToID("KW_FluidsMapWorldPosition_lod1");
            public static readonly int KWS_FluidsMaskWorldPosition      = Shader.PropertyToID("KWS_FluidsMaskWorldPosition");
            public static readonly int KW_FluidsMapAreaSize_lod0        = Shader.PropertyToID("KW_FluidsMapAreaSize_lod0");
            public static readonly int KW_FluidsMapAreaSize_lod1        = Shader.PropertyToID("KW_FluidsMapAreaSize_lod1");
            public static readonly int KW_Fluids_Lod0                   = Shader.PropertyToID("KW_Fluids_Lod0");
            public static readonly int KW_FluidsFoam_Lod0               = Shader.PropertyToID("KW_FluidsFoam_Lod0");
            public static readonly int KW_Fluids_Lod1                   = Shader.PropertyToID("KW_Fluids_Lod1");
            public static readonly int KW_FluidsFoam_Lod1               = Shader.PropertyToID("KW_FluidsFoam_Lod1");
            public static readonly int KW_FluidsRequiredReadPrebakedSim = Shader.PropertyToID("KW_FluidsRequiredReadPrebakedSim");
            public static readonly int KWS_FluidsMask_Lod0              = Shader.PropertyToID("KWS_FluidsMask_Lod0");
            public static readonly int KWS_FluidsMask_Lod1              = Shader.PropertyToID("KWS_FluidsMask_Lod1");
        }

        public static class DynamicWaves
        {
            public static readonly int KWS_DynamicWavesGlobalForceScale = Shader.PropertyToID("KWS_DynamicWavesGlobalForceScale");
            public static readonly int KWS_DynamicWavesWaterSurfaceHeight = Shader.PropertyToID("KWS_DynamicWavesWaterSurfaceHeight");
            public static readonly int KWS_DynamicWavesForce = Shader.PropertyToID("KWS_DynamicWavesForce");
            public static readonly int KWS_MeshIntersectionThreshold = Shader.PropertyToID("KWS_MeshIntersectionThreshold");


            public static readonly int KW_DynamicWavesWorldPos = Shader.PropertyToID("KW_DynamicWavesWorldPos");
            public static readonly int KW_DynamicWavesAreaSize = Shader.PropertyToID("KW_DynamicWavesAreaSize");
            public static readonly int KW_DynamicObjectsMap = Shader.PropertyToID("KW_DynamicObjectsMap");
            public static readonly int KW_InteractiveWavesPixelSpeed = Shader.PropertyToID("KW_InteractiveWavesPixelSpeed");
            public static readonly int KWS_PreviousTarget = Shader.PropertyToID("KWS_PreviousTarget");
            public static readonly int KWS_CurrentTarget = Shader.PropertyToID("KWS_CurrentTarget");
            public static readonly int KW_AreaOffset = Shader.PropertyToID("KW_AreaOffset");
            public static readonly int KW_LastAreaOffset = Shader.PropertyToID("KW_LastAreaOffset");
            public static readonly int KW_InteractiveWavesAreaSize = Shader.PropertyToID("KW_InteractiveWavesAreaSize");
            public static readonly int KWS_DynamicWaves = Shader.PropertyToID("KWS_DynamicWaves");
            public static readonly int KWS_DynamicWavesNormal = Shader.PropertyToID("KWS_DynamicWavesNormal");

        }

        public static class ShorelineID
        {
            public static readonly int KWS_ShorelineWavesDisplacement = Shader.PropertyToID("KWS_ShorelineWavesDisplacement");
            public static readonly int KWS_ShorelineWavesNormal = Shader.PropertyToID("KWS_ShorelineWavesNormal");
            public static readonly int KWS_ShorelineAreaPosSize = Shader.PropertyToID("KWS_ShorelineAreaPosSize");
            public static readonly int KWS_ShorelineAreaWavesCount = Shader.PropertyToID("KWS_ShorelineAreaWavesCount");

        }

        public static class UnderwaterID
        {
            public static readonly int KWS_TargetResolutionMultiplier = Shader.PropertyToID("KWS_TargetResolutionMultiplier");
            public static readonly int KWS_UnderwaterRT_Blured = Shader.PropertyToID("KWS_UnderwaterRT_Blured");
            public static readonly int KWS_Underwater_RTHandleScale = Shader.PropertyToID("KWS_Underwater_RTHandleScale");
        }

        public static class DrawToDepthID
        {

        }

        #endregion

        #region Shader Keywords

        public static class WaterKeywords
        {
            //public static          readonly GlobalKeyword GlobalKeyword_KWS_SSR_REFLECTION = GlobalKeyword.Create("KWS_SSR_REFLECTION");
            public static readonly GlobalKeyword GlobalKeyword_KWS_USE_VOLUMETRIC_LIGHT   = GlobalKeyword.Create("KWS_USE_VOLUMETRIC_LIGHT");
            public static readonly GlobalKeyword GlobalKeyword_KWS_USE_AQUARIUM_RENDERING = GlobalKeyword.Create("KWS_USE_AQUARIUM_RENDERING");
            public static readonly GlobalKeyword GlobalKeyword_KWS_USE_HALF_LINE_TENSION  = GlobalKeyword.Create("KWS_USE_HALF_LINE_TENSION");
         
            public static readonly string STEREO_INSTANCING_ON      = "STEREO_INSTANCING_ON";
            public static readonly string USE_WATER_INSTANCING      = "USE_WATER_INSTANCING";
            public static readonly string USE_MULTIPLE_SIMULATIONS  = "USE_MULTIPLE_SIMULATIONS";
            public static readonly string KW_FLOW_MAP_EDIT_MODE     = "KW_FLOW_MAP_EDIT_MODE";
            public static readonly string KW_FLOW_MAP               = "KW_FLOW_MAP";
            public static readonly string KW_FLOW_MAP_FLUIDS        = "KW_FLOW_MAP_FLUIDS";
            public static readonly string KW_DYNAMIC_WAVES          = "KW_DYNAMIC_WAVES";
            public static readonly string USE_CAUSTIC               = "USE_CAUSTIC";
            public static readonly string USE_ADDITIONAL_CAUSTIC    = "USE_ADDITIONAL_CAUSTIC";
            public static readonly string REFLECT_SUN               = "REFLECT_SUN";
            public static readonly string USE_SHORELINE             = "USE_SHORELINE";
            public static readonly string KWS_USE_PLANAR_REFLECTION = "KWS_USE_PLANAR_REFLECTION";
            public static readonly string KWS_SSR_REFLECTION        = "KWS_SSR_REFLECTION";
            public static readonly string KWS_USE_REFLECTION_PROBES = "KWS_USE_REFLECTION_PROBES";

            public static readonly string USE_REFRACTION_DISPERSION = "USE_REFRACTION_DISPERSION";
            public static readonly string USE_REFRACTION_IOR        = "USE_REFRACTION_IOR";
            public static readonly string USE_FOAM                  = "USE_FOAM";
            public static readonly string KWS_USE_EXTRUDE_MASK      = "KWS_USE_EXTRUDE_MASK";


            public static readonly string USE_UNDERWATER_REFLECTION           = "USE_UNDERWATER_REFLECTION";
        }

        public static class CausticKeywords
        {
            public static readonly string USE_DEPTH_SCALE = "USE_DEPTH_SCALE";
            public static readonly string USE_DISPERSION = "USE_DISPERSION";
            public static readonly string USE_CAUSTIC_FILTERING = "USE_CAUSTIC_FILTERING";
        }

        public static class ShorelineKeywords
        {
            public static readonly string FOAM_RECEIVE_SHADOWS = "FOAM_RECEIVE_SHADOWS";
            public static readonly string FOAM_COMPUTE_WATER_OFFSET = "FOAM_COMPUTE_WATER_OFFSET";
            public static readonly string KWS_FOAM_USE_FAST_PATH = "KWS_FOAM_USE_FAST_MODE";
        }

        public static class SSRKeywords
        {
            public static readonly string USE_HOLES_FILLING         = "USE_HOLES_FILLING";
        }

        public static class FogKeywords
        {

        }

        public static class UnderwaterKeywords
        {
            public static readonly string USE_AQUARIUM_MODE                = "USE_AQUARIUM_MODE";
            public static readonly string USE_PHYSICAL_APPROXIMATION_COLOR = "USE_PHYSICAL_APPROXIMATION_COLOR";
            public static readonly string USE_PHYSICAL_APPROXIMATION_SSR   = "USE_PHYSICAL_APPROXIMATION_SSR";
            public static readonly string KWS_CAMERA_UNDERWATER            = "KWS_CAMERA_UNDERWATER";
        }

        #endregion

        #region Compute kernels

        public static class SSR_Kernels
        {
            public static readonly string Clear_kernel = "Clear_kernel";
            public static readonly string RenderHash_kernel = "RenderHash_kernel";
            public static readonly string RenderColorFromHash_kernel = "RenderColorFromHash_kernel";
            public static readonly string RenderSinglePassSSR_kernel = "RenderSinglePassSSR_kernel";
            public static readonly string FillHoles_kernel = "FillHoles_kernel";
            public static readonly string TemporalFiltering_kernel = "TemporalFiltering_kernel";
        }

        #endregion

        #region StructuredBuffers

        public static class StructuredBuffers
        {
            public static readonly int InstancedMeshData = Shader.PropertyToID("InstancedMeshData");
            public static readonly int KWS_ShorelineDataBuffer = Shader.PropertyToID("KWS_ShorelineDataBuffer");
            public static readonly int KWS_DynamicWavesMaskBuffer = Shader.PropertyToID("KWS_DynamicWavesMaskBuffer");
        }

        #endregion
    }
}

