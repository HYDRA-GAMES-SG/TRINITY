using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace KWS
{
    internal class ScreenSpaceReflectionPass : WaterPass
    {
        internal override string                      PassName => "Water.ScreenSpaceReflectionPass";

        internal          Dictionary<Camera, SsrData> ssrDatas = new Dictionary<Camera, SsrData>();

        Material         _anisoMaterial;
        private Material _ssrMaterial;
        ComputeShader    _cs;


        int _kernelClear;
        int _kernelRenderHash;
        int _kernelRenderColorFromHash;

      
        private WaterSystemScriptableData.ScreenSpaceReflectionResolutionQualityEnum _lastResolutionSetting;

        private const int MaxSsrDataCameras = 5;
        const int shaderNumthreadX = 8;
        const int shaderNumthreadY = 8;

       
        public ScreenSpaceReflectionPass()
        {
            _ssrMaterial                                   =  KWS_CoreUtils.CreateMaterial(KWS_ShaderConstants.ShaderNames.SsrShaderName);
            WaterSharedResources.OnAnyWaterSettingsChanged += OnAnyWaterSettingsChanged;
        }

        public override void Release()
        {
            WaterSharedResources.OnAnyWaterSettingsChanged -= OnAnyWaterSettingsChanged;
            ReleaseSsrDatas();

            KW_Extensions.SafeDestroy(_cs, _anisoMaterial, _ssrMaterial);
        }

        private void OnAnyWaterSettingsChanged(WaterSystem waterInstance, WaterSystem.WaterTab waterTab)
        {
            if (waterTab.HasTab(WaterSystem.WaterTab.Reflection))
            {
                if (RequireReinitialize()) ReleaseSsrDatas();
            }
        }

        public bool RequireReinitialize()
        {
            var settings = WaterSharedResources.GlobalSettings;
            if (_lastResolutionSetting != settings.ScreenSpaceReflectionResolutionQuality)
            {
                _lastResolutionSetting = settings.ScreenSpaceReflectionResolutionQuality;
                return true;
            }

            return false;
        }


        void ReleaseSsrDatas()
        {
            foreach (var ssrData in ssrDatas)
            {
                ssrData.Value?.Release();
            }
            ssrDatas.Clear();
        }


        internal void InitializeShaders()
        {
            _cs = KWS_CoreUtils.LoadComputeShader(KWS_ShaderConstants.ShaderNames.SsrComputePath);
            //_cs = (ComputeShader)Resources.Load(KWS_ShaderConstants.ShaderNames.SsrComputePath);
            if (_cs != null)
            {
                _kernelClear = _cs.FindKernel("Clear_kernel");
                _kernelRenderHash = _cs.FindKernel("RenderHash_kernel");
                _kernelRenderColorFromHash = _cs.FindKernel("RenderColorFromHash_kernel");
            }
        }

        internal static Vector2Int ScaleFunc(Vector2Int size)
        {
            float scale = (int)WaterSharedResources.GlobalSettings.ScreenSpaceReflectionResolutionQuality / 100f;
            return new Vector2Int(Mathf.RoundToInt(scale * size.x), Mathf.RoundToInt(scale * size.y));
        }


        internal class SsrData
        {
            public RTHandle[]    ReflectionRT = new RTHandle[2];
            public ComputeBuffer HashBuffer;


            public int Frame;

            public Matrix4x4 PrevVPMatrix;
            public Matrix4x4[] PrevVPMatrixStereo = new Matrix4x4[2];


            public Vector2Int GetCurrentResolution()
            {
                var scale = ReflectionRT[0].rtHandleProperties.rtHandleScale;
                return new Vector2Int(Mathf.RoundToInt(ReflectionRT[0].rt.width * scale.x), Mathf.RoundToInt(ReflectionRT[0].rt.height * scale.y));
            }

            public void InitializeHashBuffer(Vector2Int resolution)
            {
                var size = resolution.x * resolution.y;
                if (KWS_CoreUtils.SinglePassStereoEnabled) size *= 2;
                HashBuffer = KWS_CoreUtils.GetOrUpdateBuffer<uint>(ref HashBuffer, size);
            }

          
            internal void InitializeTextures()
            {
                var colorFormat       = GraphicsFormat.R16G16B16A16_SFloat;
                for (int idx = 0; idx < 2; idx++)
                {
                    ReflectionRT[idx] = KWS_CoreUtils.RTHandleAllocVR(ScaleFunc, name: "_ssrReflectionRT" + idx, colorFormat: colorFormat, enableRandomWrite: true, useMipMap: true, autoGenerateMips: false, mipMapCount: 5);
                }


                this.WaterLog(ReflectionRT[0]);
            }

            public void ReleaseTextures()
            {
                ReflectionRT[0]?.Release();
                ReflectionRT[1]?.Release();

                ReflectionRT[0] = ReflectionRT[1] = null;
                
            }

            public void Release()
            {
                ReleaseTextures();

                HashBuffer?.Release();
                HashBuffer = null;

                this.WaterLog("", KW_Extensions.WaterLogMessageType.Release);
            }
        }

        public override void ExecuteCommandBuffer(WaterPass.WaterPassContext waterContext)
        {
            ExecuteRaymarchedReflection(waterContext);
            //ExecutePlanarRaymarchedReflection(waterContext); 
        }

        public void ExecuteRaymarchedReflection(WaterPass.WaterPassContext waterContext)
        {
            if (!WaterSharedResources.IsAnyWaterUseSsr) return;

            var settings = WaterSharedResources.GlobalSettings;
            var cam = waterContext.cam;
            var cmd = waterContext.cmd;

            if (ssrDatas.Count > MaxSsrDataCameras) ssrDatas.Clear();
            if (!ssrDatas.ContainsKey(cam))
            {
                ssrDatas.Add(cam, new SsrData());
            }

            var data = ssrDatas[cam];

            if (data.ReflectionRT[0] == null) data.InitializeTextures();

            var targetRT     = data.Frame % 2 == 0 ? data.ReflectionRT[0] : data.ReflectionRT[1];
            var lastTargetRT = data.Frame % 2 == 0 ? data.ReflectionRT[1] : data.ReflectionRT[0];


            var useUnderwaterReflection = settings.UseUnderwaterEffect
                                       && settings.UnderwaterReflectionMode == WaterSystemScriptableData.UnderwaterReflectionModeEnum.PhysicalAproximatedReflection
                                       && (WaterSystem.IsCameraUnderwater || WaterSharedResources.IsAnyAquariumWaterVisible)
                                       && !waterContext.cam.orthographic;

            cmd.SetKeyword(KWS_ShaderConstants.WaterKeywords.STEREO_INSTANCING_ON,      KWS_CoreUtils.SinglePassStereoEnabled && cam.stereoEnabled);
            cmd.SetKeyword(KWS_ShaderConstants.WaterKeywords.KWS_USE_PLANAR_REFLECTION, WaterSharedResources.IsAnyWaterUsePlanar);
            cmd.SetKeyword(KWS_ShaderConstants.WaterKeywords.USE_UNDERWATER_REFLECTION, useUnderwaterReflection);


            cmd.SetGlobalInt("KWS_Frame", data.Frame);
            cmd.SetGlobalFloat(KWS_ShaderConstants.ConstantWaterParams.KWS_ScreenSpaceBordersStretching, settings.ScreenSpaceBordersStretching);
            cmd.SetGlobalInt(KWS_ShaderConstants.ConstantWaterParams.KWS_OverrideSkyColor, settings.OverrideSkyColor ? 1 : 0);
            
            cmd.BlitTriangleRTHandle(lastTargetRT, targetRT, _ssrMaterial, ClearFlag.All, Color.clear, pass: 0);

            WaterSharedResources.SsrReflection                  = targetRT;
            WaterSharedResources.SsrReflectionCurrentResolution = data.GetCurrentResolution();

            cmd.SetGlobalTexture(KWS_ShaderConstants.SSR_ID.KWS_ScreenSpaceReflectionRT, WaterSharedResources.SsrReflection);
            cmd.SetGlobalVector(KWS_ShaderConstants.SSR_ID.KWS_ScreenSpaceReflection_RTHandleScale, WaterSharedResources.SsrReflection.rtHandleProperties.rtHandleScale);
            //WaterSharedResources.SsrReflectionRaw = targetRT;
            data.Frame++;
        }

        public void ExecutePlanarRaymarchedReflection(WaterPass.WaterPassContext waterContext)
        {
            if (!WaterSharedResources.IsAnyWaterUseSsr) return;

            var settings = WaterSharedResources.GlobalSettings;

            var cam = waterContext.cam;
            var cmd = waterContext.cmd;


            if (ssrDatas.Count > MaxSsrDataCameras) ssrDatas.Clear();
            if (!ssrDatas.ContainsKey(cam))
            {
                ssrDatas.Add(cam, new SsrData());
            }

            var data = ssrDatas[cam];

            if (_cs == null) InitializeShaders();
            if (data.ReflectionRT[0] == null) data.InitializeTextures();

            if (_cs == null) return;

            var targetRT = data.Frame % 2 == 0 ? data.ReflectionRT[0] : data.ReflectionRT[1];
            var lastTargetRT = data.Frame % 2 == 0 ? data.ReflectionRT[1] : data.ReflectionRT[0];

            var currentResolution = data.GetCurrentResolution();

            var dispatchSize = new Vector2Int(Mathf.CeilToInt((float)currentResolution.x / shaderNumthreadX), Mathf.CeilToInt((float)currentResolution.y / shaderNumthreadY));

            var stereoPasses = KWS_CoreUtils.SinglePassStereoEnabled ? 2 : 1;
            var ssrInstances = WaterSharedResources.KWS_ActiveSsrInstancesCount;
            if (targetRT.rt.volumeDepth != stereoPasses) return;
            data.InitializeHashBuffer(currentResolution);

            var useUnderwaterReflection = settings.UseUnderwaterEffect 
                                       && settings.UnderwaterReflectionMode == WaterSystemScriptableData.UnderwaterReflectionModeEnum.PhysicalAproximatedReflection 
                                       && (WaterSystem.IsCameraUnderwater || WaterSharedResources.IsAnyAquariumWaterVisible)
                                       && !waterContext.cam.orthographic;
            
            _cs.SetKeyword(KWS_ShaderConstants.WaterKeywords.STEREO_INSTANCING_ON,      KWS_CoreUtils.SinglePassStereoEnabled);
            _cs.SetKeyword(KWS_ShaderConstants.SSRKeywords.USE_HOLES_FILLING,           settings.UseScreenSpaceReflectionHolesFilling);
            _cs.SetKeyword(KWS_ShaderConstants.WaterKeywords.USE_UNDERWATER_REFLECTION,   useUnderwaterReflection);
            _cs.SetKeyword(KWS_ShaderConstants.WaterKeywords.KWS_USE_PLANAR_REFLECTION, WaterSharedResources.IsAnyWaterUsePlanar);


            CoreUtils.SetRenderTarget(waterContext.cmd, targetRT, ClearFlag.Color, Color.clear);

            cmd.SetComputeIntParam(_cs, KWS_ShaderConstants.ConstantWaterParams.UseScreenSpaceReflectionSky, settings.UseScreenSpaceReflectionSky ? 1 : 0);
            cmd.SetGlobalInt(KWS_ShaderConstants.ConstantWaterParams.KWS_OverrideSkyColor, settings.OverrideSkyColor ? 1 : 0);
            cmd.SetComputeIntParam(_cs, KWS_ShaderConstants.ReflectionsID.KWS_ReprojectedFrameReady, data.Frame > 5 ? 1 : 0);

            cmd.SetComputeVectorParam(_cs, KWS_ShaderConstants.SSR_ID._RTSize, new Vector4(currentResolution.x, currentResolution.y, 1f / currentResolution.x, 1f / currentResolution.y));
            cmd.SetGlobalFloatArray(KWS_ShaderConstants.ArrayWaterParams.KWS_ActiveSsrInstances, WaterSharedResources.KWS_ActiveSsrInstances1);
            cmd.SetGlobalFloat(KWS_ShaderConstants.SSR_ID.KWS_AverageInstancesHeight, GetAverageSsrInstancesHeight());
            cmd.SetGlobalFloat(KWS_ShaderConstants.ConstantWaterParams.KWS_ScreenSpaceBordersStretching, settings.ScreenSpaceBordersStretching);

            if (stereoPasses == 2) cmd.SetComputeMatrixArrayParam(_cs, KWS_ShaderConstants.CameraMatrix.KWS_PREV_MATRIX_VP_STEREO, data.PrevVPMatrixStereo);
            else cmd.SetComputeMatrixParam(_cs, KWS_ShaderConstants.CameraMatrix.KWS_PREV_MATRIX_VP, data.PrevVPMatrix);

            ///////////////////////////clear pass//////////////////////////////////
            cmd.SetComputeBufferParam(_cs, _kernelClear, KWS_ShaderConstants.SSR_ID.HashRT, data.HashBuffer);
            cmd.SetComputeTextureParam(_cs, _kernelClear, KWS_ShaderConstants.SSR_ID.ColorRT, targetRT.rt);
            cmd.DispatchCompute(_cs, _kernelClear, dispatchSize.x, dispatchSize.y, stereoPasses);
            //////////////////////////////////////////////////////////////////////


            ///////////////////////////render to hash pass//////////////////////////////////
            cmd.SetComputeBufferParam(_cs, _kernelRenderHash, KWS_ShaderConstants.SSR_ID.HashRT, data.HashBuffer);
            cmd.SetComputeTextureParam(_cs, _kernelRenderHash, KWS_ShaderConstants.SSR_ID.ColorRT, targetRT.rt);
            cmd.SetComputeTextureParam(_cs, _kernelRenderHash, KWS_ShaderConstants.SSR_ID._CameraDepthTexture, waterContext.cameraDepth);
            cmd.DispatchCompute(_cs, _kernelRenderHash, dispatchSize.x, dispatchSize.y, ssrInstances);
            ////////////////////////////////////////////////////////////////////////////////


            ///////////////////////////render color pass//////////////////////////////////

            cmd.SetComputeTextureParam(_cs, _kernelRenderColorFromHash, KWS_ShaderConstants.ReflectionsID.KWS_PlanarReflectionRT, WaterSharedResources.PlanarReflection);
            cmd.SetComputeTextureParam(_cs, _kernelRenderColorFromHash, KWS_ShaderConstants.SSR_ID.KWS_LastTargetRT,              lastTargetRT);
            cmd.SetComputeBufferParam(_cs, _kernelRenderColorFromHash, KWS_ShaderConstants.SSR_ID.HashRT, data.HashBuffer);
            cmd.SetComputeTextureParam(_cs, _kernelRenderColorFromHash, KWS_ShaderConstants.SSR_ID.ColorRT, targetRT.rt);

            cmd.SetComputeTextureParam(_cs, _kernelRenderColorFromHash, KWS_ShaderConstants.SSR_ID._CameraOpaqueTexture, waterContext.cameraColor);

            cmd.DispatchCompute(_cs, _kernelRenderColorFromHash, dispatchSize.x, dispatchSize.y, stereoPasses);
            //////////////////////////////////////////////////////////////////////////////


            data.Frame++;
            if (stereoPasses == 2) data.PrevVPMatrixStereo = WaterSystem.CurrentVPMatrixStereo;
            else data.PrevVPMatrix = WaterSystem.CurrentVPMatrix;

            WaterSharedResources.SsrReflection = targetRT;
            WaterSharedResources.SsrReflectionCurrentResolution = currentResolution;

            cmd.SetGlobalTexture(KWS_ShaderConstants.SSR_ID.KWS_ScreenSpaceReflectionRT, WaterSharedResources.SsrReflection);
            cmd.SetGlobalVector(KWS_ShaderConstants.SSR_ID.KWS_ScreenSpaceReflection_RTHandleScale, WaterSharedResources.SsrReflection.rtHandleProperties.rtHandleScale);
            //WaterSharedResources.SsrReflectionRaw = targetRT;
        }

        float GetAverageSsrInstancesHeight()
        {
            var averageInstancesHeight = 0f;
            for (int i = 0; i < WaterSharedResources.KWS_ActiveSsrInstancesCount; i++)
            {
                var ssrInstanceIdx = WaterSharedResources.KWS_ActiveSsrInstances1[i];
                averageInstancesHeight += WaterSharedResources.WaterInstances[(int)ssrInstanceIdx - 1].WaterPivotWorldPosition.y;
            }

            return averageInstancesHeight / (float)WaterSharedResources.KWS_ActiveSsrInstancesCount;
        }
    }
}