using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using static KWS.WaterSystemScriptableData;
using static KWS.KW_Extensions;
using static KWS.KWS_ShaderConstants;

namespace KWS
{
    public partial class WaterSystem
    {

        #region Initialization


        //If you press ctrl+z after deleting the water gameobject, unity returns all objects without links and save all objects until you close the editor. Not sure how to fix that =/ 
        void ClearUndoObjects(Transform parent)
        {
            if (parent.childCount > 0)
            {
                KW_Extensions.SafeDestroy(parent.GetChild(0).gameObject);
            }
        }

        internal void CheckAndUpdateShaderParams()
        {
#if UNITY_EDITOR
            var mat = InstanceData.GetCurrentWaterMaterial();
            if (mat != null && !mat.HasProperty(ConstantWaterParams.KW_Transparent))
            {
                //Debug.Log("CheckAndUpdateShaderParams");
                InstanceData.UpdateInstanceMaterialsConstantParams();
            }
#endif
        }

        internal void UpdateState(WaterTab changedTab)
        {
            OnWaterSettingsChanged?.Invoke(this, changedTab);
        }


        private void OnCurrentWaterSettingsChangedEvent(WaterSystem instance, WaterTab changedTab)
        {
            UpdateWaterInstance(changedTab);
            WaterSharedResources.OnAnyWaterSettingsChanged?.Invoke(instance, changedTab);
        }

        private void UpdateWaterInstance(WaterTab changedTab)
        {
            UpdateOtherWaterInstancesGlobalSettings(changedTab);

            if (changedTab.HasTab(WaterTab.Transform))
            {
                SetScaleRotationRelativeToMeshType();
                WorldSpaceBounds = CalculateCurrentWorldSpaceBounds(true);
            }

            if (changedTab.HasTab(WaterTab.TransformWaterLevel))
            {
                WaterPivotWorldPosition.y = WaterRootTransform.position.y;
                WorldSpaceBounds        = CalculateCurrentWorldSpaceBounds(true);
            }

            if (changedTab.HasTab(WaterTab.Waves))
            {
                CurrentMaxWaveHeight = EvaluateMaxAmplitute();
                WorldSpaceBounds = CalculateCurrentWorldSpaceBounds(true);
            }

            if (changedTab.HasTab(WaterTab.Mesh) || changedTab.HasTab(WaterTab.Transform))
            {
                RebuildMesh();
            }

            if (changedTab.HasTab(WaterTab.Mesh) || changedTab.HasTab(WaterTab.Transform) || changedTab.HasTab(WaterTab.TransformWaterLevel))
            {
                if(_aabbMeshFilter.sharedMesh != null) KW_Extensions.SafeDestroy(_aabbMeshFilter.sharedMesh);
                _aabbMeshFilter.sharedMesh = MeshUtils.CreateMeshFromBounds(WorldSpaceBounds, WaterSize, WaterPivotWorldPosition);
            }

            InstanceData.UpdateInstanceMaterialsConstantParams();
            UpdateSharedResources();
            LoadSharedResourceTextures();
        }

        private void OnWaterInstancesCountChangedEvent()
        {
            WaterShaderPassID = WaterSharedResources.WaterInstances.IndexOf(this) + 1;
            UpdateSharedResources();
            if(IsWaterInitialized) InstanceData.UpdateInstanceID();
        }

        private void UpdateSharedResources()
        {
            WaterSharedResources.IsAnyWaterUseGlobalWind = WaterSharedResources.WaterInstances.Any(w => w.Settings.UseGlobalWind);

            WaterSharedResources.IsAnyWaterUseIntersectionFoam = WaterSharedResources.WaterInstances.Any(w => w.Settings.UseIntersectionFoam);
            WaterSharedResources.IsAnyWaterUseOceanFoam        = WaterSharedResources.WaterInstances.Any(w => w.Settings.UseOceanFoam);
            WaterSharedResources.IsAnyWaterUseFluidsFoam       = WaterSharedResources.WaterInstances.Any(w => w.Settings.UseFlowMap && w.Settings.UseFluidsSimulation);

            WaterSharedResources.IsAnyWaterUseSsr                              = WaterSharedResources.WaterInstances.Any(w => w.Settings.UseScreenSpaceReflection);
            WaterSharedResources.IsAnyWaterUsePlanar                           = WaterSharedResources.WaterInstances.Any(w => w.Settings.UsePlanarReflection);
            WaterSharedResources.IsAnyWaterUseShoreline                        = WaterSharedResources.WaterInstances.Any(w => w.Settings.UseShorelineRendering);
            WaterSharedResources.IsAnyWaterUseCaustic                          = WaterSharedResources.WaterInstances.Any(w => w.Settings.UseCausticEffect);
            WaterSharedResources.IsAnyWaterUseVolumetricLightAdditionalCaustic = WaterSharedResources.WaterInstances.Any(w => w.Settings.VolumetricLightUseAdditionalLightsCaustic);
            WaterSharedResources.IsAnyWaterUseFlowmap                          = WaterSharedResources.WaterInstances.Any(w => w.Settings.UseFlowMap);
            WaterSharedResources.IsAnyWaterUseVolumetricLighting               = WaterSharedResources.WaterInstances.Any(w => w.Settings.UseVolumetricLight);
            WaterSharedResources.IsAnyWaterUseDynamicWaves                     = WaterSharedResources.WaterInstances.Any(w => w.Settings.UseDynamicWaves);
            WaterSharedResources.IsAnyWaterUseDrawToDepth                      = WaterSharedResources.WaterInstances.Any(w => w.Settings.DrawToPosteffectsDepth);
            WaterSharedResources.IsAnyWaterUseAquariumMode = WaterSharedResources.WaterInstances.Any(w => w.Settings.WaterMeshType is WaterMeshTypeEnum.FiniteBox or WaterMeshTypeEnum.CustomMesh);

            WaterSharedResources.KWS_WindSpeedArray[WaterShaderPassID]                   = Settings.CurrentWindSpeed;
            WaterSharedResources.KWS_TransparentArray[WaterShaderPassID]                 = Settings.Transparent;
            WaterSharedResources.KWS_UnderwaterTransparentOffsetArray[WaterShaderPassID] = Settings.OverrideUnderwaterTransparent ?  Settings.UnderwaterTransparentOffset : 0;
            WaterSharedResources.KWS_DyeColorArray[WaterShaderPassID]                    = Settings.DyeColor;
            WaterSharedResources.KWS_TurbidityColorArray[WaterShaderPassID]              = Settings.TurbidityColor;
            WaterSharedResources.KWS_WaterPositionArray[WaterShaderPassID]               = WaterPivotWorldPosition;
            WaterSharedResources.KWS_WavesAreaScaleArray[WaterShaderPassID]              = Settings.CurrentWavesAreaScale;
            WaterSharedResources.KWS_CausticStrengthArray[WaterShaderPassID]             = Settings.CausticStrength;
            //WaterSharedResources.KWS_VolumetricLightIntensityArray[WaterShaderPassID] = new Vector4(Settings.VolumetricLightDirLightIntensityMultiplier, Settings.VolumetricLightAdditionalLightsIntensityMultiplier);
            WaterSharedResources.KWS_MeshTypeArray[WaterShaderPassID]                 = (int)Settings.WaterMeshType;

            if (Settings.UsePlanarReflection) WaterSharedResources.PlanarInstanceID = WaterShaderPassID;

            if (WaterSharedResources.IsAnyWaterUseCaustic)
            {
                var waterWithCaustics = WaterSharedResources.WaterInstances.Where(w => w.Settings.UseCausticEffect).ToList();
                WaterSharedResources.MaxCausticResolution  = waterWithCaustics.Max(w => (int)w.Settings.GetCurrentCausticTextureResolutionQuality);
                WaterSharedResources.MaxCausticArraySlices = waterWithCaustics.Count;
            }

            if (WaterSharedResources.IsAnyWaterUseSsr)
            {
                var ssrIndex = 0;
                foreach (var waterInstance in WaterSharedResources.WaterInstances)
                {
                    if (waterInstance.Settings.UseScreenSpaceReflection)
                    {
                        WaterSharedResources.KWS_ActiveSsrInstances1[ssrIndex] = waterInstance.WaterShaderPassID;
                        ssrIndex++;
                    }
                }

                WaterSharedResources.KWS_ActiveSsrInstancesCount = ssrIndex;
            }
        }


        private static void LoadSharedResourceTextures()
        {
            if (WaterSharedResources.IsAnyWaterUseIntersectionFoam)
            {
                if (WaterSharedResources.KWS_IntersectionFoamTex == null) WaterSharedResources.KWS_IntersectionFoamTex = Resources.Load<Texture2D>(KWS_Settings.ResourcesPaths.KWS_IntersectionFoamTex);
                Shader.SetGlobalTexture("KWS_IntersectionFoamTex", WaterSharedResources.KWS_IntersectionFoamTex);
            }

            if (WaterSharedResources.IsAnyWaterUseOceanFoam)
            {
                if (WaterSharedResources.KWS_OceanFoamTex == null) WaterSharedResources.KWS_OceanFoamTex = Resources.Load<Texture2D>(KWS_Settings.ResourcesPaths.KWS_OceanFoamTex);
                Shader.SetGlobalTexture("KWS_OceanFoamTex", WaterSharedResources.KWS_OceanFoamTex);
            }

            if (WaterSharedResources.IsAnyWaterUseFluidsFoam)
            {
                if (WaterSharedResources.KWS_FluidsFoamTex == null) WaterSharedResources.KWS_FluidsFoamTex = Resources.Load<Texture2D>(KWS_Settings.ResourcesPaths.KWS_FluidsFoamTex);
                Shader.SetGlobalTexture("KW_FluidsFoamTex", WaterSharedResources.KWS_FluidsFoamTex);
            }
        }

        private void AddWaterInstance()
        {
            SetCurrentWaterInstanceSettingsFromGlobalSettings();
            WaterSharedResources.WaterInstances.Add(this);
            if (Settings.WaterMeshType == WaterMeshTypeEnum.InfiniteOcean) WaterSharedResources.WaterInstances.Swap(0, WaterSharedResources.WaterInstances.Count - 1);
            WaterSharedResources.OnWaterInstancesCountChanged?.Invoke();
        }

        private void RemoveWaterInstance()
        {
            WaterSharedResources.WaterInstances.Remove(this);
            WaterSharedResources.OnWaterInstancesCountChanged?.Invoke();
        }

        float FftSmoothstep(float x, float scaleX, float scaleY)
        {
            if (x > scaleX) return scaleY;
            return (-2 * Mathf.Pow(x / scaleX, 3) + 3 * Mathf.Pow(x / scaleX, 2)) * scaleY;
        }

        float EvaluateMaxAmplitute()
        {
            int cascades = Settings.CurrentFftWavesCascades;
            float windSpeed = Settings.CurrentWindSpeed;
            float windTurbulence = Settings.CurrentWindTurbulence;
            float scale = Settings.CurrentWavesAreaScale * 0.5f;

            //main formula -2x^3+3x^2 -> (-2(x/scaleX)^3+3(x/scaleX)^2)*scaleY

            //  4 cascades, 1-50 windspeed, turbulence 0.0 scaleXY(35, 9.5)
            //  4 cascades, 1-50 windspeed, turbulence 1.0 scaleXY(40, 19.0)

            //  3 cascades, 1-50 windspeed, turbulence 0.0 scaleXY(18, 1.5)
            //  3 cascades, 1-50 windspeed, turbulence 1.0 scaleXY(22, 3.0)

            //  2 cascades, 1-50 windspeed, turbulence 0.0 scaleXY(8.5, 0.38)
            //  2 cascades, 1-50 windspeed, turbulence 1.0 scaleXY(8.5, 0.51)

            //  1 cascades, 1-50 windspeed, turbulence 0.0 scaleXY(4, 0.115)
            //  1 cascades, 1-50 windspeed, turbulence 1.0 scaleXY(4, 0.135)

            if (cascades == 4)
            {
                return scale * FftSmoothstep(windSpeed, Mathf.Lerp(35, 40, windTurbulence), Mathf.Lerp(9.5f, 23.0f, windTurbulence));
            }
            else if (cascades == 3)
            {
                return scale * FftSmoothstep(windSpeed, Mathf.Lerp(18, 22, windTurbulence), Mathf.Lerp(1.5f, 3.0f, windTurbulence));
            }
            else if (cascades == 2)
            {
                return scale * FftSmoothstep(windSpeed, Mathf.Lerp(8.5f, 8.5f, windTurbulence), Mathf.Lerp(0.38f, 0.51f, windTurbulence));
            }
            else if (cascades == 1)
            {
                return scale * FftSmoothstep(windSpeed, Mathf.Lerp(4, 4, windTurbulence), Mathf.Lerp(0.115f, 0.135f, windTurbulence));
            }


            //Debug.Log($"amplitude {amplitude}");

            return 1;

            //cascade | turbulence | wind speed | max amplitude
            //5 | 0 | 50 | 10
            //5 | 0.5 | 50 | 20
            //5 | 1 | 50 | 28
        }


        void CreateInfiniteOceanShadowCullingAnchor()
        {
            _infiniteOceanShadowCullingAnchor = KW_Extensions.CreateHiddenGameObject("InfiniteOceanShadowCullingAnchor");
            var shadowFixInfiniteOceanMaterial = KWS_CoreUtils.CreateMaterial(KWS_ShaderConstants.ShaderNames.InfiniteOceanShadowCullingAnchorName);
            var shadowFixInfiniteOceanMesh = KWS_CoreUtils.CreateQuad();

            _infiniteOceanShadowCullingAnchor.AddComponent<MeshRenderer>().sharedMaterial = shadowFixInfiniteOceanMaterial;
            _infiniteOceanShadowCullingAnchor.AddComponent<MeshFilter>().sharedMesh = shadowFixInfiniteOceanMesh;


            _infiniteOceanShadowCullingAnchor.transform.rotation = Quaternion.Euler(270, 0, 0);
            _infiniteOceanShadowCullingAnchor.transform.localScale = new Vector3(100000, 100000, 1);
            _infiniteOceanShadowCullingAnchor.transform.parent = UpdateManagerObject.transform;
            _infiniteOceanShadowCullingAnchorTransform = _infiniteOceanShadowCullingAnchor.transform;
        }

        void UpdateInfiniteOceanShadowCullingAnchor()
        {
            var camPos = _currentCamera.GetCameraPositionFast();
            var pos = WaterRelativeWorldPosition;
            pos.y = Mathf.Min(pos.y, camPos.y) - 250;
            _infiniteOceanShadowCullingAnchorTransform.position = pos;
            _infiniteOceanShadowCullingAnchorTransform.rotation = Quaternion.Euler(270, 0, 0);
        }


        internal void InitializeRiverMesh()
        {
            SplineMeshComponent.CreateMeshFromSpline(this);
            SplineRiverMesh = SplineMeshComponent.CurrentMesh;
        }

        internal void InitializeOrUpdateMesh()
        {
            WorldSpaceBounds = CalculateCurrentWorldSpaceBounds(useWavesAmplitudeOffset: true);

            switch (Settings.WaterMeshType)
            {
                case WaterMeshTypeEnum.InfiniteOcean:
                    _meshQuadTree.Initialize(MeshQuadTree.QuadTreeTypeEnum.Infinite, this);
                    break;
                case WaterMeshTypeEnum.FiniteBox:
                    _meshQuadTree.Initialize(MeshQuadTree.QuadTreeTypeEnum.Finite, this);
                    break;
                case WaterMeshTypeEnum.River:
                    InitializeRiverMesh();
                    break;
            }
        }


        internal Bounds CalculateCurrentWorldSpaceBounds(bool useWavesAmplitudeOffset)
        {
            var    amplitudeOffset = useWavesAmplitudeOffset ? CurrentMaxWaveHeight : 0;
            Bounds bounds;
            switch (Settings.WaterMeshType)
            {
                case WaterMeshTypeEnum.InfiniteOcean:
                    {
                        bounds = new Bounds(WaterPivotWorldPosition + new Vector3(0, -KWS_Settings.Mesh.MaxInfiniteOceanDepth + amplitudeOffset, 0),
                                          new Vector3(1000000, KWS_Settings.Mesh.MaxInfiniteOceanDepth * 2, 1000000));
                        break;
                    }
                case WaterMeshTypeEnum.FiniteBox:
                    {
                        bounds = KW_Extensions.BoundsLocalToWorld(new Bounds(Vector3.zero, Vector3.one), WaterRootTransform, -0.5f, amplitudeOffset);
                        break;
                    }
                case WaterMeshTypeEnum.River:
                    {
                        bounds = KW_Extensions.BoundsLocalToWorld(SplineMeshComponent.GetBounds(), WaterRootTransform, -0.5f, amplitudeOffset);
                        break;
                    }
                case WaterMeshTypeEnum.CustomMesh:
                {
                    bounds = (Settings.CustomMesh == null)
                        ? new Bounds(WaterPivotWorldPosition, Vector3.one)
                        : KW_Extensions.BoundsLocalToWorld(Settings.CustomMesh.bounds, WaterRootTransform, 0, amplitudeOffset);
                    break;
                }
                default: return new Bounds(WaterPivotWorldPosition, Vector3.one);
            }

            WaterBoundsSurfaceHeight = bounds.max.y;
            return bounds;
        }

        internal void RebuildMesh()
        {
            if (_currentCamera != null) InitializeOrUpdateMesh();
        }


        void SetScaleRotationRelativeToMeshType()
        {
            switch (Settings.WaterMeshType)
            {
                case WaterMeshTypeEnum.InfiniteOcean:
                    WaterRootTransform.rotation   = Quaternion.identity;
                    WaterRootTransform.localScale = Vector3.one;
                    break;
                case WaterMeshTypeEnum.FiniteBox:
                    WaterRootTransform.rotation = Quaternion.Euler(0, WaterRootTransform.eulerAngles.y, 0);
                    break;
                case WaterMeshTypeEnum.River:
                    WaterRootTransform.rotation = Quaternion.identity;
                    WaterRootTransform.localScale = Vector3.one;
                    break;
                case WaterMeshTypeEnum.CustomMesh:
                    WaterRootTransform.rotation = Quaternion.Euler(0, WaterRootTransform.eulerAngles.y, 0);
                    break;
            }
            WaterRootTransform.localScale = Vector3.Max(KWS_Settings.Mesh.MinFiniteSize, WaterRootTransform.localScale);

            WaterPivotWorldRotation = WaterRootTransform.rotation;
            WaterPivotWorldPosition = WaterRootTransform.position;
            WaterSize = WaterRootTransform.localScale;
        }

        private float _lastFiniteWaterLevel;
        internal void UpdateWaterTransformsData()
        {
            if (WaterRootTransform.hasChanged)
            {
                transform.hasChanged = false;

                if (Application.isPlaying)
                {
                    switch (Settings.WaterMeshType)
                    {
                        case WaterMeshTypeEnum.CustomMesh:
                            ForceUpdateWaterSettings(WaterTab.Transform);
                            break;
                        case WaterMeshTypeEnum.FiniteBox:
                        {
                            var isWaterLevelChanged = Math.Abs(_lastFiniteWaterLevel - WaterRootTransform.position.y) > 0.001f;
                            if (isWaterLevelChanged)
                            {
                                _lastFiniteWaterLevel = WaterRootTransform.position.y;
                                ForceUpdateWaterSettings(WaterTab.TransformWaterLevel);
                            }
                            break;
                        }
                    }
                }
                else
                {
                    ForceUpdateWaterSettings(WaterTab.Transform);
                }
            }
        }

       
        void InitializeWaterCommonResources()
        {
            InitializeOrUpdateMesh();

            IsWaterInitialized = true;
        }

        bool IsWorldPosUnderwaterAccurate(Vector3 worldPos)
        {
            var newPos = GetCurrentWaterSurfaceData(worldPos);
            if (!newPos.IsActualDataReady) return false;

            return worldPos.y < newPos.Position.y;
        }

        internal float GetCurrentWaterSurfaceHeightFast(Vector3 worldPosition)
        {
            if (Settings.WaterMeshType == WaterMeshTypeEnum.River)
            {
                worldPosition.y = WaterPivotWorldPosition.y;
                return SplineMeshComponent.GetSplineSurfaceHeight(this, worldPosition);
            }
            else
            {
                return WaterPivotWorldPosition.y;
            }
        }

        static void UpdateWaterVisibilityForAllInstances()
        {
            WaterSharedResources.IsAnyAquariumWaterVisible = false;

            foreach (var waterInstance in WaterSharedResources.WaterInstances)
            {
                if (waterInstance.IsWaterInitialized)
                {
                    if (!WaterSharedResources.FrustumCaches.TryGetValue(_currentCamera, out var frustumCache)) return;

                    waterInstance.IsWaterVisible = IsBoxVisibleAccurate(ref frustumCache.FrustumPlanes, ref frustumCache.FrustumCorners,
                                                                        waterInstance.WorldSpaceBounds.min,          waterInstance.WorldSpaceBounds.max);

                    if (waterInstance.IsWaterVisible && waterInstance.Settings.CanUseAquariumMode)
                    {
                        WaterSharedResources.IsAnyAquariumWaterVisible = true;
                    }
                }
            }
        }

        static void UpdateUnderwaterStateForAllInstances(Camera cam)
        {
            var points = CalculateNearPlaneWorldPoints(cam);
          
            IsCameraUnderwater = false;
            foreach (var waterInstance in WaterSharedResources.WaterInstances) waterInstance.IsCameraUnderwaterForInstance = false;

            foreach (var waterInstance in WaterSharedResources.WaterInstances)
            {
                if (!KWS_CoreUtils.IsWaterVisibleAndActive(waterInstance)) continue;

                if (waterInstance.Settings.WaterMeshType == WaterMeshTypeEnum.River)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        if (waterInstance.SplineMeshComponent.GetSplineSurfaceHeight(waterInstance, points[i], out var surfaceWorldPos, out _))
                        {
                            if (points[i].y < surfaceWorldPos.y + waterInstance.CurrentMaxWaveHeight)
                            {
                                waterInstance.IsCameraUnderwaterForInstance = true;
                                IsCameraUnderwater = true;
                                return;
                            }
                        }
                    }
                }
                else
                {
                    if (IsCameraNearPlaneInsideAABB(points, waterInstance.WorldSpaceBounds))
                    {
                        waterInstance.IsCameraUnderwaterForInstance = true;
                        IsCameraUnderwater = true;
                        return;
                    }
                }
            }
        }

        internal List<ReflectionProbeBlendInfo> GetAffectedReflectionProbes()
        {

            _aabbRenderer.GetClosestReflectionProbes(_afectedProbes);
            return _afectedProbes;
        }


        #endregion

        #region Render Logic 


        void RenderWater()
        {
            if (_currentCamera == null) return;

            UpdateWaterTransformsData();

            if (!IsWaterInitialized) InitializeWaterCommonResources();
            if (!isWaterPlatformSpecificResourcesInitialized) InitializeWaterPlatformSpecificResources();

            if (Settings.WaterMeshType == WaterMeshTypeEnum.InfiniteOcean)
            {
                if (_infiniteOceanShadowCullingAnchor == null) CreateInfiniteOceanShadowCullingAnchor();
                UpdateInfiniteOceanShadowCullingAnchor();
            }

            var currentDeltaTime = KW_Extensions.DeltaTime();
            //if (currentDeltaTime < 0.0001f) return;

            //if (Settings.WaterMeshType == WaterMeshTypeEnum.InfiniteOcean || Settings.WaterMeshType == WaterMeshTypeEnum.FiniteBox)
            //{
            //    _meshQuadTree.UpdateQuadTree(_currentCamera, this);
            //    if (_meshQuadTree.TryGetRenderingContext(_currentCamera, false, out var context))
            //    {
            //        var mat = InstanceData.GetCurrentWaterMaterial();
            //        mat.SetBuffer(StructuredBuffers.InstancedMeshData, context.visibleChunksComputeBuffer);
            //    }
            //}

            PlanarReflectionComponent.RenderReflection(this, _currentCamera);

        }


        internal static Matrix4x4   CurrentVPMatrix;
        internal static Matrix4x4[]   CurrentInversePMatrix = new Matrix4x4[2];
        internal static Matrix4x4[] CurrentVPMatrixStereo = new Matrix4x4[2];
        //private Matrix4x4 _prevVPMatrix;
        //Matrix4x4[] _prevVPMatrixStereo = new Matrix4x4[2];

        void SetGlobalShaderParams()
        {
            Shader.SetGlobalFloat(ConstantWaterParams.KWS_SunMaxValue,                  KWS_Settings.Reflection.MaxSunStrength);
            Shader.SetGlobalFloat(ConstantWaterParams.KWS_AbsorbtionOverrideMultiplier, KWS_Settings.VolumetricLighting.AbsorbtionOverrideMultiplier);
            Shader.SetGlobalFloatArray(ConstantWaterParams.KWS_WavesDomainSizes, KWS_Settings.FFT.FftDomainSizes);
            Shader.SetGlobalVectorArray(ConstantWaterParams.KWS_WavesDomainScales, KWS_Settings.FFT.FftDomainScales);
            Shader.SetGlobalFloatArray(ConstantWaterParams.KWS_WavesDomainVisiableArea, KWS_Settings.FFT.FftDomainVisiableArea);
        }

        static void SetGlobalCameraShaderParams(Camera cam)
        {
            var cameraProjectionMatrix = cam.projectionMatrix;
            if (KWS_CoreUtils.SinglePassStereoEnabled)
            {
                var arr_matrix_I_VP = new Matrix4x4[2];
                for (uint eyeIndex = 0; eyeIndex < 2; eyeIndex++)
                {
                    var matrix_cameraProjeciton = cam.GetStereoProjectionMatrix((Camera.StereoscopicEye)eyeIndex);
                    var matrix_V = GL.GetGPUProjectionMatrix(matrix_cameraProjeciton, true);
                    var matrix_P = cam.GetStereoViewMatrix((Camera.StereoscopicEye)eyeIndex);
                    var matrix_VP = matrix_V * matrix_P;

                    CurrentInversePMatrix[eyeIndex] = matrix_P.inverse;
                    CurrentVPMatrixStereo[eyeIndex] = matrix_VP;
                    arr_matrix_I_VP[eyeIndex]       = matrix_VP.inverse;
                }
                Shader.SetGlobalMatrixArray(CameraMatrix.KWS_MATRIX_I_P_STEREO, CurrentInversePMatrix);
                Shader.SetGlobalMatrixArray(CameraMatrix.KWS_MATRIX_VP_STEREO, CurrentVPMatrixStereo);
                Shader.SetGlobalMatrixArray(CameraMatrix.KWS_MATRIX_I_VP_STEREO, arr_matrix_I_VP);
            }
            else
            {
                var matrix_V = GL.GetGPUProjectionMatrix(cameraProjectionMatrix, true);
                var maitix_P = cam.worldToCameraMatrix;
                CurrentVPMatrix = matrix_V * maitix_P;

                Shader.SetGlobalMatrix(CameraMatrix.KWS_MATRIX_I_P,  maitix_P.inverse);
                Shader.SetGlobalMatrix(CameraMatrix.KWS_MATRIX_VP,   CurrentVPMatrix);
                Shader.SetGlobalMatrix(CameraMatrix.KWS_MATRIX_I_VP, CurrentVPMatrix.inverse);
            }


            Shader.SetGlobalVector(DynamicWaterParams.KWS_CameraForward, cam.GetCameraForwardFast());

            Shader.SetGlobalInt(DynamicWaterParams.KWS_UnderwaterVisible, IsCameraUnderwater ? 1 : 0);
            Shader.SetGlobalFloat(DynamicWaterParams.KW_Time, KW_Extensions.TotalTime());
            Shader.SetGlobalFloat(ConstantWaterParams.KW_GlobalTimeScale, WaterSharedResources.GlobalSettings.GlobalTimeScale);

            Shader.SetGlobalInteger(ConstantWaterParams.KWS_WaterLayerMask, KWS_Settings.Water.WaterLayer);
            Shader.SetGlobalInteger(ConstantWaterParams.KWS_WaterLightLayerMask, KWS_Settings.Water.LightLayer);
        }

        static void UpdateArrayShaderParams()
        {
            Shader.SetGlobalFloat(ArrayWaterParams.KWS_WaterInstancesCount, WaterSharedResources.WaterInstances.Count);
            Shader.SetGlobalFloatArray(ArrayWaterParams.KWS_WindSpeedArray, WaterSharedResources.KWS_WindSpeedArray);

            Shader.SetGlobalFloatArray(ArrayWaterParams.KWS_TransparentArray,                 WaterSharedResources.KWS_TransparentArray);
            Shader.SetGlobalFloatArray(ArrayWaterParams.KWS_UnderwaterTransparentOffsetArray, WaterSharedResources.KWS_UnderwaterTransparentOffsetArray);
            Shader.SetGlobalVectorArray(ArrayWaterParams.KWS_DyeColorArray,     WaterSharedResources.KWS_DyeColorArray);
            Shader.SetGlobalVectorArray(ArrayWaterParams.KWS_TurbidityColorArray, WaterSharedResources.KWS_TurbidityColorArray);

            Shader.SetGlobalVectorArray(ArrayWaterParams.KWS_WaterPositionArray,                  WaterSharedResources.KWS_WaterPositionArray);
            Shader.SetGlobalFloatArray(ArrayWaterParams.KWS_CausticStrengthArray, WaterSharedResources.KWS_CausticStrengthArray);

            Shader.SetGlobalFloatArray(ArrayWaterParams.KWS_WavesAreaScaleArray, WaterSharedResources.KWS_WavesAreaScaleArray);
            Shader.SetGlobalFloatArray(ArrayWaterParams.KWS_MeshTypeArray, WaterSharedResources.KWS_MeshTypeArray);

            Shader.SetGlobalInt(KWS_ShaderConstants.ReflectionsID.KWS_PlanarReflectionInstanceID, WaterSharedResources.PlanarInstanceID);
        }

        internal void UpdateWaterInstanceID(Material mat)
        {
            mat.SetInt(DynamicWaterParams.KWS_WaterInstanceID, WaterShaderPassID);
        }

        internal void SetConstantShaderParamsShared(Material mat, bool isPrePass)
        {
            if (mat == null || Settings == null) return;

            //#if KWS_DEBUG
            //            this.WaterLog("SetConstantShaderParamsShared " + shader.name, WaterLogMessageType.StaticUpdate);
            //#endif

            var isFlowmapUsed         = Settings.UseFlowMap && !Settings.UseFluidsSimulation;
            var isFlowmapFluidsUsed   = Settings.UseFlowMap && Settings.UseFluidsSimulation;
            var useInstancedRendering = Settings.WaterMeshType == WaterMeshTypeEnum.FiniteBox || Settings.WaterMeshType == WaterMeshTypeEnum.InfiniteOcean;

            if (isPrePass == false)
            {
                KWS_CoreUtils.SetKeywords(mat, (WaterKeywords.USE_WATER_INSTANCING, useInstancedRendering));
            }

            KWS_CoreUtils.SetKeywords(mat,
                                      (WaterKeywords.STEREO_INSTANCING_ON, KWS_CoreUtils.SinglePassStereoEnabled),
                                      (WaterKeywords.KWS_SSR_REFLECTION, Settings.UseScreenSpaceReflection),
                                      (WaterKeywords.KWS_USE_PLANAR_REFLECTION, Settings.UsePlanarReflection),
                                      (WaterKeywords.KWS_USE_REFLECTION_PROBES, Settings.UseReflectionProbes),
                                      (WaterKeywords.REFLECT_SUN, Settings.ReflectSun),
                                      (WaterKeywords.USE_REFRACTION_IOR, Settings.RefractionMode == RefractionModeEnum.PhysicalAproximationIOR),
                                      (WaterKeywords.USE_REFRACTION_DISPERSION, Settings.UseRefractionDispersion),
                                      (WaterKeywords.KW_FLOW_MAP_EDIT_MODE, FlowMapInEditMode),
                                      (WaterKeywords.KW_FLOW_MAP, isFlowmapUsed),
                                      (WaterKeywords.KW_FLOW_MAP_FLUIDS, isFlowmapFluidsUsed),
                                      (WaterKeywords.KW_DYNAMIC_WAVES, Settings.UseDynamicWaves),
                                      (WaterKeywords.USE_SHORELINE, Settings.UseShorelineRendering),
                                      //(WaterKeywords.USE_VOLUMETRIC_LIGHT, Settings.UseVolumetricLight),
                                      (WaterKeywords.USE_CAUSTIC, Settings.UseCausticEffect)
                );



            KWS_CoreUtils.SetFloats(mat,
                                    (ConstantWaterParams.KW_Transparent, Settings.Transparent),
                                    (ConstantWaterParams.KWS_WindSpeed, Settings.CurrentWindSpeed),
                                    (ConstantWaterParams.KWS_WindRotation, Settings.CurrentWindRotation),
                                    (ConstantWaterParams.KWS_WindTurbulence, Settings.CurrentWindTurbulence),

                                    (ConstantWaterParams.KWS_WavesCascades, Settings.CurrentFftWavesCascades),
                                    (ConstantWaterParams.KWS_WavesAreaScale, Settings.CurrentWavesAreaScale),
                                    (ConstantWaterParams.KW_WaterFarDistance, Settings.OceanDetailingFarDistance),
                                    //(ConstantWaterParams.KW_GlobalTimeScale, Settings.TimeScale),


                                    // (ConstantWaterParams.KWS_IgnoreAnisotropicScreenSpaceSky, Settings.UseAnisotropicReflections && Settings.UseAnisotropicCubemapSkyForSSR ? 1 : 0),
                                    // (ConstantWaterParams.KWS_AnisoReflectionsScale, AnisoScaleRelativeToWind),
                                    (ConstantWaterParams.KWS_SkyLodRelativeToWind, SkyLodRelativeToWind),
                                    (ConstantWaterParams.KW_ReflectionClipOffset, Settings.ReflectionClipPlaneOffset),
                                    (ConstantWaterParams.KWS_SunCloudiness, Settings.ReflectedSunCloudinessStrength),
                                    (ConstantWaterParams.KWS_SunStrength, Settings.ReflectedSunStrength),

                                    (ConstantWaterParams.KWS_RefractionAproximatedDepth, Settings.RefractionAproximatedDepth),
                                    (ConstantWaterParams.KWS_RefractionSimpleStrength, Settings.RefractionSimpleStrength),
                                    (ConstantWaterParams.KWS_RefractionDispersionStrength, Settings.RefractionDispersionStrength * KWS_Settings.Water.MaxRefractionDispersion),
                                    (ConstantWaterParams.KW_WaterFarDistance, Settings.OceanDetailingFarDistance),
                                    (ConstantWaterParams.KWS_WaterYRotationRad, WaterPivotWorldRotation.eulerAngles.y * Mathf.Deg2Rad),
                                    (ConstantWaterParams.KWS_UnderwaterHalfLineTensionScale, Settings.UnderwaterHalfLineTensionScale)
                );

            KWS_CoreUtils.SetInts(mat,
                                  (DynamicWaterParams.KWS_WaterInstanceID, WaterShaderPassID),
                                  (ConstantWaterParams.KWS_OverrideSkyColor, Settings.OverrideSkyColor ? 1 : 0),
                                  (ConstantWaterParams.KWS_MeshType, (int)Settings.WaterMeshType),
                                  (ConstantWaterParams.KWS_UseOceanFoam, Settings.UseOceanFoam ? 1 : 0),
                                  (ConstantWaterParams.KWS_UseIntersectionFoam, Settings.UseIntersectionFoam ? 1 : 0),
                                  (ConstantWaterParams.KWS_UseRefractionIOR, Settings.RefractionMode == RefractionModeEnum.PhysicalAproximationIOR ? 1 : 0),
                                  (ConstantWaterParams.KWS_UseRefractionDispersion, Settings.UseRefractionDispersion ? 1 : 0),
                                  (ConstantWaterParams.KWS_UseWireframeMode, Settings.WireframeMode ? 1 : 0),
                                  (ConstantWaterParams.KWS_UseFilteredNormals, (int)Settings.CurrentFftWavesQuality <= 64 ? 1 : 0));


            KWS_CoreUtils.SetVectors(mat,
                                     (DynamicWaterParams.KW_WaterPosition, WaterPivotWorldPosition),
                                     (ConstantWaterParams.KWS_DyeColor, Settings.DyeColor),
                                     (ConstantWaterParams.KW_TurbidityColor, Settings.TurbidityColor),
                                     (ConstantWaterParams.KWS_CustomSkyColor, Settings.CustomSkyColor),
                                     (ConstantWaterParams.KWS_InstancingWaterScale, WaterSize));

            //KWS_CoreUtils.SetMatrices(shader, (ConstantWaterParams.KWS_InstancingRotationMatrix, Matrix4x4.TRS(Vector3.zero, WaterPivotWorldRotation, Vector3.one)));

            if (Settings.UseFlowMap)
            {
                var fluidsSpeed = Settings.FlowMapSpeed;
                //if (Settings.UseFluidsSimulation && !FlowMapInEditMode) fluidsSpeed = Settings.FluidsSpeed * Mathf.Lerp(0.125f, 1.0f, Settings.FluidsSimulationIterrations / 4.0f);

                KWS_CoreUtils.SetVectors(mat, (ConstantWaterParams.KW_FlowMapOffset, Settings.FlowMapAreaPosition));
                KWS_CoreUtils.SetFloats(mat, (ConstantWaterParams.KW_FlowMapSize, Settings.FlowMapAreaSize),
                                        (ConstantWaterParams.KW_FlowMapSpeed, fluidsSpeed),
                                        (ConstantWaterParams.KW_FlowMapFluidsStrength, Settings.FluidsFoamStrength));

            }


            if (Settings.UseDynamicWaves)
            {
                KWS_CoreUtils.SetFloats(mat, (DynamicWaves.KW_DynamicWavesAreaSize, Settings.DynamicWavesAreaSize));
            }

            if (Settings.UseShorelineRendering)
            {
            }

            if (Settings.UseOceanFoam)
            {
                KWS_CoreUtils.SetVectors(mat, (ConstantWaterParams.KWS_OceanFoamStrengthSize, new Vector2(Settings.OceanFoamStrength, 1.0f / Settings.OceanFoamTextureSize)));
                KWS_CoreUtils.SetVectors(mat, (ConstantWaterParams.KWS_OceanFoamColor, Settings.OceanFoamColor));

            }

            if (Settings.UseIntersectionFoam)
            {
                KWS_CoreUtils.SetVectors(mat, (ConstantWaterParams.KWS_IntersectionFoamFadeSize, new Vector2(1.0f / Settings.IntersectionFoamFadeDistance, 1.0f / Settings.IntersectionTextureFoamSize)),
                                         (ConstantWaterParams.KWS_IntersectionFoamColor, Settings.IntersectionFoamColor));

            }

            //if (Settings.UseCausticEffect)
            //{
            //    KWS_CoreUtils.SetFloats(shader, (CausticID.KW_DecalScale, KWS_Settings.Caustic.LodSettings[Settings.CausticActiveLods - 1] * 2));
            //    KWS_CoreUtils.SetKeywords(shader,
            //                              (CausticKeywords.USE_LOD1, Settings.CausticActiveLods == 2),
            //                              (CausticKeywords.USE_LOD2, Settings.CausticActiveLods == 3),
            //                              (CausticKeywords.USE_LOD3, Settings.CausticActiveLods == 4),
            //                              (CausticKeywords.USE_DEPTH_SCALE, Settings.UseDepthCausticScale));
            //}

            if (Settings.UseUnderwaterEffect)
            {
            }


            if (Settings.UseTesselation)
            {
                KWS_CoreUtils.SetFloats(mat, (ConstantWaterParams._TesselationMaxDisplace, Mathf.Max(Settings.CurrentWindSpeed, 2)),
                                        (ConstantWaterParams._TesselationFactor, Settings.CurrentTesselationFactor),
                                        (ConstantWaterParams._TesselationMaxDistance, Settings.CurrentTesselationMaxDistance)
                );
            }
        }

        #endregion

    }

}