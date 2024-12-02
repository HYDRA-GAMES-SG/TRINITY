using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using static KWS.KWS_CoreUtils;
using static KWS.KWS_ShaderConstants;


namespace KWS
{
    internal class VolumetricLightingPass : WaterPass
    {
        internal override string PassName => "Water.VolumetricLightingPass";

        Material                 _volumeMaterial;

        internal      Dictionary<Camera, VolumetricData>                             _volumetricDatas  = new Dictionary<Camera, VolumetricData>();
        private const int                                                            MaxVolumetricDataCameras = 5;
        private       WaterSystemScriptableData.VolumetricLightResolutionQualityEnum _lastResolutionSetting;



        public VolumetricLightingPass()
        {
            _volumeMaterial        = CreateMaterial(ShaderNames.VolumetricLightingShaderName);
            WaterSharedResources.OnAnyWaterSettingsChanged += OnAnyWaterSettingsChanged;
        }


        internal class VolumetricData
        {
            public RTHandle[] VolumetricLightRT = new RTHandle[2];
            public RTHandle   VolumetricLightAdditionalDataRT;
            public int        Frame;

            public Matrix4x4   PrevVPMatrix;
            public Matrix4x4[] PrevVPMatrixStereo = new Matrix4x4[2];

            internal void InitializeTextures()
            {
                var resolutionDownsample                                 = (int)WaterSharedResources.GlobalSettings.VolumetricLightResolutionQuality / 100f;
                var maxSize                                              = KWS_CoreUtils.GetScreenSizeLimited(false);
                var height                                               = (int)(maxSize.y * resolutionDownsample);
                var width                                                = (int)(height    * 2); // typical resolution ratio is 16x9 (or 2x1), for better pixel filling we use [2 * width] x [height], instead of square [width] * [height].
                var hdrFormat                                            = GetGraphicsFormatHDR();
                for (int idx = 0; idx < 2; idx++) VolumetricLightRT[idx] = KWS_CoreUtils.RTHandleAllocVR(width, height, name: "_volumeLightRT" + idx, colorFormat: hdrFormat);
                VolumetricLightAdditionalDataRT = KWS_CoreUtils.RTHandleAllocVR(width, height, name: "_volumeLightAdditionalDataRT", colorFormat: GraphicsFormat.R8G8B8A8_UNorm);
                this.WaterLog(VolumetricLightRT[0]);
            }

            internal void Update()
            {
                Frame++;
                if (Frame > int.MaxValue - 1) Frame = 0;

                if (KWS_CoreUtils.SinglePassStereoEnabled) PrevVPMatrixStereo = WaterSystem.CurrentVPMatrixStereo;
                else PrevVPMatrix                                             = WaterSystem.CurrentVPMatrix;
            }

            internal void ReleaseTextures()
            {
                VolumetricLightRT[0]?.Release();
                VolumetricLightRT[1]?.Release();
                VolumetricLightAdditionalDataRT?.Release();

                VolumetricLightRT[0] = VolumetricLightRT[1] = VolumetricLightAdditionalDataRT = null;
            }

            internal void Release()
            {
                ReleaseTextures();
                this.WaterLog("", KW_Extensions.WaterLogMessageType.Release);
            }
        }

        public bool RequireReinitialize()
        {
            if (_lastResolutionSetting == WaterSharedResources.GlobalSettings.VolumetricLightResolutionQuality) return false;
            _lastResolutionSetting = WaterSharedResources.GlobalSettings.VolumetricLightResolutionQuality;
            return true;

        }

        private void OnAnyWaterSettingsChanged(WaterSystem instance, WaterSystem.WaterTab changedTabs)
        {
            if (changedTabs.HasTab(WaterSystem.WaterTab.VolumetricLighting))
            {
                if (RequireReinitialize()) ReleaseVolumetricDatas();
            }
        }


        void ReleaseVolumetricDatas()
        {
            foreach (var data in _volumetricDatas)
            {
                data.Value?.Release();
            }
            _volumetricDatas.Clear();
        }

        public override void Release()
        {
            WaterSharedResources.OnAnyWaterSettingsChanged -= OnAnyWaterSettingsChanged;
            ReleaseVolumetricDatas();
            KW_Extensions.SafeDestroy(_volumeMaterial);
            

            this.WaterLog(string.Empty, KW_Extensions.WaterLogMessageType.Release);
        }
        public override void ExecuteCommandBuffer(WaterPass.WaterPassContext waterContext)
        {
            if (!WaterSharedResources.IsAnyWaterUseVolumetricLighting) return;

            var cmd = waterContext.cmd;
            var cam = waterContext.cam;
           

            if (_volumetricDatas.Count > MaxVolumetricDataCameras) _volumetricDatas.Clear();
            if (!_volumetricDatas.ContainsKey(cam))
            {
                _volumetricDatas.Add(cam, new VolumetricData());
            }
            var data = _volumetricDatas[cam];
            if (data.VolumetricLightRT[0] == null) data.InitializeTextures();


            var targetRT     = data.Frame % 2 == 0 ? data.VolumetricLightRT[0] : data.VolumetricLightRT[1];
            var lastTargetRT = data.Frame % 2 == 0 ? data.VolumetricLightRT[1] : data.VolumetricLightRT[0];
            UpdateShaderParams(waterContext.cam, cmd, data, targetRT, data.VolumetricLightAdditionalDataRT, lastTargetRT);
            
            CoreUtils.SetRenderTarget(waterContext.cmd, KWS_CoreUtils.GetMrt(targetRT, data.VolumetricLightAdditionalDataRT), targetRT, ClearFlag.Color, Color.black);
            cmd.BlitTriangle(_volumeMaterial);
           
            data.Update();
            WaterSharedResources.VolumetricLightingRT               = targetRT;
            WaterSharedResources.VolumetricLightingAdditionalDataRT = data.VolumetricLightAdditionalDataRT;
            //Shader.SetGlobalVector(VolumetricLightConstantsID.KWS_VolumetricLight_RTHandleScale, WaterSharedResources.VolumetricLightingRT.rtHandleProperties.rtHandleScale);
            //Shader.SetGlobalTexture(VolumetricLightConstantsID.KWS_VolumetricLightRT, WaterSharedResources.VolumetricLightingRT);
        }


        Vector4 ComputeMieVector(float mieG)
        {
            return new Vector4(1 - (mieG * mieG), 1 + (mieG * mieG), 2 * mieG, 1.0f / (4.0f * Mathf.PI));
        }

        private void UpdateShaderParams(Camera cam, CommandBuffer cmd, VolumetricData data, RTHandle targetRT, RTHandle additionalLightsDataRT, RTHandle lastTargetRT)
        {
            var anisoMie = ComputeMieVector(0.8f);
         cmd.SetGlobalVector(VolumetricLightConstantsID.KWS_LightAnisotropy, anisoMie);
            cmd.SetGlobalFloat(VolumetricLightConstantsID.KWS_VolumetricLightTemporalAccumulationFactor, WaterSharedResources.GlobalSettings.VolumetricLightTemporalReprojectionAccumulationFactor);
           
            cmd.SetGlobalInteger(VolumetricLightConstantsID.KWS_RayMarchSteps, WaterSharedResources.GlobalSettings.VolumetricLightIteration);

            var useCaustic = WaterSharedResources.IsAnyWaterUseCaustic && !WaterSharedResources.IsAnyWaterUseVolumetricLightAdditionalCaustic;
            var useAdditionalCaustic = WaterSharedResources.IsAnyWaterUseCaustic && WaterSharedResources.IsAnyWaterUseVolumetricLightAdditionalCaustic;

            if (WaterSharedResources.GlobalSettings.VolumetricLightResolutionQuality == WaterSystemScriptableData.VolumetricLightResolutionQualityEnum.Low ||
                WaterSharedResources.GlobalSettings.VolumetricLightResolutionQuality == WaterSystemScriptableData.VolumetricLightResolutionQualityEnum.VeryLow)
            {
                useCaustic = false;
                useAdditionalCaustic = false;
            }


            var settings = WaterSharedResources.GlobalSettings;
            //var useUnderwaterReflection = settings.UseUnderwaterEffect
            //                           && settings.UnderwaterReflectionMode == WaterSystemScriptableData.UnderwaterReflectionModeEnum.PhysicalAproximatedReflection
            //                           && (WaterSystem.IsCameraUnderwater || WaterSharedResources.IsAnyAquariumWaterVisible)
            //                           && !cam.orthographic;


            cmd.SetKeyword(WaterKeywords.USE_CAUSTIC,                                 useCaustic);
            cmd.SetKeyword(WaterKeywords.USE_ADDITIONAL_CAUSTIC,                      useAdditionalCaustic);
            //cmd.SetKeyword(WaterKeywords.USE_UNDERWATER_REFLECTION, useUnderwaterReflection);

            if (KWS_CoreUtils.SinglePassStereoEnabled) cmd.SetGlobalMatrixArray(KWS_ShaderConstants.CameraMatrix.KWS_PREV_MATRIX_VP_STEREO, data.PrevVPMatrixStereo);
            else cmd.SetGlobalMatrix(KWS_ShaderConstants.CameraMatrix.KWS_PREV_MATRIX_VP, data.PrevVPMatrix);

            cmd.SetGlobalTexture(KWS_ShaderConstants.VolumetricLightConstantsID.KWS_VolumetricLightRT_Last, lastTargetRT);
            cmd.SetGlobalVector(VolumetricLightConstantsID.KWS_VolumetricLightRT_Last_RTHandleScale, lastTargetRT.rtHandleProperties.rtHandleScale);
            cmd.SetGlobalInt(KWS_ShaderConstants.VolumetricLightConstantsID.KWS_Frame, data.Frame);
           

            cmd.SetGlobalTexture(VolumetricLightConstantsID.KWS_VolumetricLightRT,               targetRT);
            cmd.SetGlobalTexture(VolumetricLightConstantsID.KWS_VolumetricLightAdditionalDataRT, additionalLightsDataRT);
            cmd.SetGlobalVector(VolumetricLightConstantsID.KWS_VolumetricLight_RTHandleScale, targetRT.rtHandleProperties.rtHandleScale);


            //var currentSize                        = targetRT.GetScaledSize(targetRT.rtHandleProperties.currentViewportSize);
            //var targetSize                         = targetRT.rtHandleProperties.currentViewportSize;
            //var downscaleFactor = new Vector2((float)targetSize.x / currentSize.x, (float)targetSize.y / currentSize.y);
            //cmd.SetGlobalVector(VolumetricLightConstantsID.KWS_VolumetricLightDownscaleFactor, downscaleFactor);
        }



    }
}