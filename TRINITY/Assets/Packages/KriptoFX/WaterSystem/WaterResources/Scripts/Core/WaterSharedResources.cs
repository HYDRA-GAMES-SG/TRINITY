using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace KWS
{
    public class WaterSharedResources
    {
        private const int MaxWaterInstances = 32;

        public static List<WaterSystem> WaterInstances { get; internal set; } = new List<WaterSystem>();
        public static Action<WaterSystem, WaterSystem.WaterTab> OnAnyWaterSettingsChanged;
        internal static WaterSystemScriptableData GlobalSettings
        {
            get
            {
                if (WaterInstances.Count > 0) return WaterInstances[0].Settings;
                return null;
            }
        }

        internal static WaterSystem GlobalWaterSystem
        {
            get
            {
                if (WaterInstances.Count > 0) return WaterInstances[0];
                return null;
            }
        }

        internal static Action OnWaterInstancesCountChanged;

        internal class CameraFrustumCache
        {
            public Plane[]   FrustumPlanes  = new Plane[6];
            public Vector3[] FrustumCorners = new Vector3[8];

            public void Update(Camera cam)
            {
                GeometryUtility.CalculateFrustumPlanes(cam, FrustumPlanes);
                KW_Extensions.CalculateFrustumCorners(ref FrustumCorners, cam);
            }
        }

        internal static Dictionary<Camera, CameraFrustumCache> FrustumCaches = new Dictionary<Camera, CameraFrustumCache>();
        internal static ReflectionProbe[]                      ReflectionProbesCache;
        internal static List<KWS_ExtrudeVolume>                ExtrudeVolumes = new List<KWS_ExtrudeVolume>();

        internal static void UpdateReflectionProbeCache()
        {
            //wtf, why urp/hdrp version doesnt have FindObjectsByType????
            //ReflectionProbesCache = UnityEngine.Object.FindObjectsByType<ReflectionProbe>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            ReflectionProbesCache = UnityEngine.Object.FindObjectsOfType<ReflectionProbe>();
        }

        internal static bool IsAnyWaterUseGlobalWind;
        internal static bool IsAnyWaterUseIntersectionFoam;
        internal static bool IsAnyWaterUseOceanFoam;
        internal static bool IsAnyWaterUseSsr;
        internal static bool IsAnyWaterUsePlanar;
        internal static bool IsAnyWaterUseFluidsFoam;
        internal static bool IsAnyWaterUseVolumetricLighting;
        internal static bool IsAnyWaterUseShoreline;
        internal static bool IsAnyWaterUseCaustic;
        internal static bool IsAnyWaterUseVolumetricLightAdditionalCaustic;
        internal static bool IsAnyWaterUseFlowmap;
        internal static bool IsAnyWaterUseDynamicWaves;
        internal static bool IsAnyWaterUseDrawToDepth;
        internal static bool IsAnyWaterUseAquariumMode;

        internal static bool IsAnyAquariumWaterVisible;

        internal static int MaxCausticResolution;
        internal static int MaxCausticArraySlices;
        internal static float[] InstanceToCausticID = new float[MaxWaterInstances];

        internal static float[]   KWS_TransparentArray    = new float[MaxWaterInstances];
        internal static float[]   KWS_UnderwaterTransparentOffsetArray    = new float[MaxWaterInstances];
        internal static Vector4[] KWS_DyeColorArray       = new Vector4[MaxWaterInstances];
        internal static Vector4[] KWS_TurbidityColorArray = new Vector4[MaxWaterInstances];

        internal static float[]   KWS_WindSpeedArray       = new float[MaxWaterInstances];
        internal static Vector4[] KWS_WaterPositionArray   = new Vector4[MaxWaterInstances];
        internal static float[]   KWS_WavesAreaScaleArray  = new float[MaxWaterInstances];
        internal static float[]   KWS_CausticStrengthArray = new float[MaxWaterInstances];
        //internal static Vector4[] KWS_VolumetricLightIntensityArray = new Vector4[MaxWaterInstances];
        internal static float[]   KWS_MeshTypeArray                 = new float[MaxWaterInstances];

        internal static float[] KWS_ActiveSsrInstances1 = new float[MaxWaterInstances];
        internal static int     KWS_ActiveSsrInstancesCount;

        internal static int GlobalWindBuoyancyLeftFramesAfterRequest = Int32.MaxValue;

        internal static Texture2D KWS_FluidsFoamTex;
        internal static Texture2D KWS_IntersectionFoamTex;
        internal static Texture2D KWS_OceanFoamTex;

        internal static RTHandle CameraOpaqueTexture;

        #region PrePass

        public static RTHandle WaterPrePassRT0                  { get; internal set; } //(X) water surface instance ID (1-255) encoded to 8bit, (Y) water face mask (1=horizon, 0.75=front face, 0.5=back face), (Z) subsurface scattering, (W) tension mask (if enabled)
        public static RTHandle WaterPrePassRT1                  { get; internal set; } //(XY) world normal xz
        public static RTHandle WaterDepthRT      { get; internal set; }
        public static RTHandle WaterIntersectionHalfLineTensionMaskRT { get; internal set; }

        public static RTHandle WaterBackfacePrePassRT0 { get; internal set; }
        public static RTHandle WaterBackfacePrePassRT1 { get; internal set; }
        public static RTHandle WaterBackfaceDepthRT    { get; internal set; }

        public static RTHandle WaterMotionVectors       { get; internal set; }

        #endregion

        #region FftWaves

        internal static RTHandle FftWavesDisplacement;
        internal static RTHandle FftWavesNormal;
        internal static RTHandle FftBuoyancyHeight;

        public static AsyncTextureSynchronizer<FftGPUHeightData> FftGpuHeightAsyncData { get; internal set; }

        public static int FftHeightDataTexSize { get; internal set; }

        public static RTHandle GetFftWavesDisplacementTexture(WaterSystem instance)
        {
            return instance.Settings.UseGlobalWind ? FftWavesDisplacement : instance.InstanceData.FftWavesTextures.GetCurrentDisplacement();
        }

        public static RTHandle GetFftWavesNormalTexture(WaterSystem instance)
        {
            return instance.Settings.UseGlobalWind ? FftWavesNormal : instance.InstanceData.FftWavesTextures.GetCurrentTargetNormal();
        }

        public static RTHandle GetFftHeightTexture(WaterSystem instance)
        {
            return instance.Settings.UseGlobalWind ? FftBuoyancyHeight : instance.InstanceData.FftBuoyancyHeight;
        }

        #endregion

        #region Reflection

        //internal static RTHandle SsrReflectionRaw;
        public static RTHandle   SsrReflection                  { get; internal set; }
        public static Vector2Int SsrReflectionCurrentResolution { get; internal set; }

        public static RenderTexture PlanarReflection { get; internal set; }
        public static int           PlanarInstanceID { get; internal set; }

        #endregion

        #region VolumetricLighting

        public static RTHandle VolumetricLightingRT { get; internal set; }
        public static RTHandle VolumetricLightingAdditionalDataRT { get; internal set; } //(R) dir light shadow for water surface + scene, (G) additional lights attenuation with shadow

        #endregion

        #region Caustic

        public static RTHandle CausticRTArray { get; internal set; }

        #endregion

        #region Shoreline
        public static Vector4  ShorelineAreaPosSize       { get; internal set; }
        public static RTHandle ShorelineWavesDisplacement { get; internal set; }
        public static RTHandle ShorelineWavesNormal       { get; internal set; }

        public static RTHandle ShorelineFoamParticlesRT { get; internal set; }

        #endregion

        #region DynamicWaves

        public static RTHandle DynamicWavesMaskRT { get; internal set; }
        public static RTHandle DynamicWavesRT { get; internal set; }

        #endregion

        #region OrthoDepth

        public static RenderTexture OrthoDepth            { get; internal set; }
        public static Vector4       OrthoDepthPosition    { get; internal set; }
        public static Vector4       OrthoDepthNearFarSize { get; internal set; }

        #endregion

        #region WriteToPostfxDepth
        

        #endregion

    }
}
