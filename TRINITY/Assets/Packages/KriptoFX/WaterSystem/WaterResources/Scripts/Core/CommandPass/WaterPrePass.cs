using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using static KWS.KWS_CoreUtils;
using static KWS.KWS_ShaderConstants;
using static KWS.WaterSystemScriptableData;

namespace KWS
{
    internal class WaterPrePass : WaterPass
    {
        internal override string  PassName => "Water.PrePass";

        readonly          Vector2 _rtScale         = new Vector2(0.5f,  0.5f);
        readonly          Vector2 _rtScaleTension  = new Vector2(0.35f, 0.35f);
        readonly          Vector2 _rtScaleAquarium = new Vector2(0.5f,  0.5f);

        private KW_PyramidBlur _pyramidBlur = new KW_PyramidBlur();
        private RTHandle       _tempIntersectionTensionRT;

     
        void InitializePrepassTextures()
        {
            if (WaterSharedResources.WaterPrePassRT0 != null) return;

            WaterSharedResources.WaterPrePassRT0 = KWS_CoreUtils.RTHandleAllocVR(_rtScale, name: "_waterPrePassRT0", colorFormat: GraphicsFormat.R8G8B8A8_UNorm);
            WaterSharedResources.WaterPrePassRT1 = KWS_CoreUtils.RTHandleAllocVR(_rtScale, name: "_waterPrePassRT1", colorFormat: GraphicsFormat.R16G16_SNorm);
            WaterSharedResources.WaterDepthRT    = KWS_CoreUtils.RTHandleAllocVR(_rtScale, name: "_waterDepthRT",    depthBufferBits: DepthBits.Depth24);
           
            Shader.SetGlobalTexture(MaskPassID.KWS_WaterPrePassRT0, WaterSharedResources.WaterPrePassRT0);
            Shader.SetGlobalTexture(MaskPassID.KWS_WaterPrePassRT1, WaterSharedResources.WaterPrePassRT1);
            Shader.SetGlobalTexture(MaskPassID.KWS_WaterDepthRT,    WaterSharedResources.WaterDepthRT);

            this.WaterLog(WaterSharedResources.WaterPrePassRT0, WaterSharedResources.WaterPrePassRT1, WaterSharedResources.WaterDepthRT);
        }

        void InitializeIntersectionHalflineTensionTextures()
        {
            if (WaterSharedResources.WaterIntersectionHalfLineTensionMaskRT != null) return;

            WaterSharedResources.WaterIntersectionHalfLineTensionMaskRT = KWS_CoreUtils.RTHandleAllocVR(_rtScaleTension, name: "_waterIntersectionHalfLineTensionMaskRT", colorFormat: GraphicsFormat.R8_UNorm);
            _tempIntersectionTensionRT                                  = KWS_CoreUtils.RTHandleAllocVR(_rtScaleTension, name: "_tempIntersectionTensionRT",              colorFormat: GraphicsFormat.R8_UNorm);
            Shader.SetGlobalTexture(MaskPassID.KWS_WaterIntersectionHalfLineTensionMaskRT, WaterSharedResources.WaterIntersectionHalfLineTensionMaskRT);
        }


        void InitializeBackfaceTextures()
        {
            if (WaterSharedResources.WaterBackfacePrePassRT0 != null) return;

            WaterSharedResources.WaterBackfacePrePassRT0 = KWS_CoreUtils.RTHandleAllocVR(_rtScaleAquarium, name: "_waterAquariumBackfacePrePassRT0", colorFormat: GraphicsFormat.R8G8B8A8_UNorm);
            WaterSharedResources.WaterBackfacePrePassRT1 = KWS_CoreUtils.RTHandleAllocVR(_rtScaleAquarium, name: "_waterAquariumBackfacePrePassRT1", colorFormat: GraphicsFormat.R16G16_SNorm);
            WaterSharedResources.WaterBackfaceDepthRT = KWS_CoreUtils.RTHandleAllocVR(_rtScaleAquarium, name: "_waterAquariumBackfaceDepthRT", depthBufferBits: DepthBits.Depth24);

            Shader.SetGlobalTexture(MaskPassID.KWS_WaterBackfacePrePassRT0, WaterSharedResources.WaterBackfacePrePassRT0);
            Shader.SetGlobalTexture(MaskPassID.KWS_WaterBackfacePrePassRT1, WaterSharedResources.WaterBackfacePrePassRT1);
            Shader.SetGlobalTexture(MaskPassID.KWS_WaterBackfaceDepthRT,    WaterSharedResources.WaterBackfaceDepthRT);



            this.WaterLog(WaterSharedResources.WaterBackfacePrePassRT0, WaterSharedResources.WaterBackfacePrePassRT1, WaterSharedResources.WaterBackfaceDepthRT);
        }

        void ReleaseTextures()
        {
            WaterSharedResources.WaterPrePassRT0?.Release();
            WaterSharedResources.WaterPrePassRT1?.Release();
            WaterSharedResources.WaterDepthRT?.Release();
            WaterSharedResources.WaterIntersectionHalfLineTensionMaskRT?.Release();

            WaterSharedResources.WaterBackfacePrePassRT0?.Release();
            WaterSharedResources.WaterBackfacePrePassRT1?.Release();
            WaterSharedResources.WaterBackfaceDepthRT?.Release();

            WaterSharedResources.WaterPrePassRT0                 = WaterSharedResources.WaterPrePassRT1                 = WaterSharedResources.WaterDepthRT = WaterSharedResources.WaterIntersectionHalfLineTensionMaskRT = null;
            WaterSharedResources.WaterBackfacePrePassRT0 = WaterSharedResources.WaterBackfacePrePassRT1 = WaterSharedResources.WaterBackfaceDepthRT = null;

            _tempIntersectionTensionRT?.Release();
            _tempIntersectionTensionRT = null;

            this.WaterLog(string.Empty, KW_Extensions.WaterLogMessageType.ReleaseRT);
        }

        public override void Release()
        {
            ReleaseTextures();
            _pyramidBlur.Release();

            this.WaterLog(string.Empty, KW_Extensions.WaterLogMessageType.Release);
        }

        public override void ExecuteCommandBuffer(WaterPass.WaterPassContext waterContext)
        {
            ExecutePrepass(waterContext);
            if(WaterSharedResources.IsAnyAquariumWaterVisible) ExecuteBackfacePrepass(waterContext);
        }

        void ExecutePrepass(WaterPass.WaterPassContext waterContext)
        {
            var settings                = WaterSharedResources.GlobalSettings;
            var globalWater             = WaterSharedResources.GlobalWaterSystem;
            var globalMat               = globalWater.InstanceData.MaterialWaterPrePass;

            var useIntersectionHalfline = settings.UseUnderwaterEffect && settings.UseUnderwaterHalfLineTensionEffect                           && WaterSystem.IsCameraUnderwater;
            var useOceanUnderwater      = settings.UseUnderwaterEffect && globalWater.Settings.WaterMeshType == WaterMeshTypeEnum.InfiniteOcean && WaterSystem.IsCameraUnderwater;
           
            InitializePrepassTextures();
            if (useIntersectionHalfline) InitializeIntersectionHalflineTensionTextures();
            waterContext.cmd.SetGlobalVector(MaskPassID.KWS_WaterPrePass_RTHandleScale, WaterSharedResources.WaterPrePassRT0.rtHandleProperties.rtHandleScale);

            CoreUtils.SetRenderTarget(waterContext.cmd, KWS_CoreUtils.GetMrt(WaterSharedResources.WaterPrePassRT0, WaterSharedResources.WaterPrePassRT1), WaterSharedResources.WaterDepthRT, 
                                      ClearFlag.All, Color.clear);
            //draw far plane volume underwater mask for the ocean
            if (useOceanUnderwater)
            {
                waterContext.cmd.SetGlobalFloat(PrePass.KWS_OceanLevel, globalWater.WaterPivotWorldPosition.y);
                CoreUtils.SetRenderTarget(waterContext.cmd, WaterSharedResources.WaterPrePassRT0, ClearFlag.None, Color.clear);
                waterContext.cmd.BlitTriangle(globalMat, pass: 4);
            }


            CoreUtils.SetRenderTarget(waterContext.cmd, KWS_CoreUtils.GetMrt(WaterSharedResources.WaterPrePassRT0, WaterSharedResources.WaterPrePassRT1), WaterSharedResources.WaterDepthRT, 
                                      ClearFlag.None, Color.clear);
            foreach (var waterInstance in WaterSharedResources.WaterInstances)
            {
                if (IsRequireRenderPrePass(waterInstance)) ExecuteInstance(waterContext.cam, waterContext.cmd, waterInstance, isBackface: false);
            }

            if (useIntersectionHalfline)
            {
                waterContext.cmd.BlitTriangleRTHandle(WaterSharedResources.WaterPrePassRT0, _tempIntersectionTensionRT, globalMat, ClearFlag.Color, Color.clear, pass: 5);
                var scale = settings.UnderwaterHalfLineTensionScale;
                scale = Mathf.Lerp(1f, 3f, scale);
                _pyramidBlur.ComputeBlurPyramid(scale, _tempIntersectionTensionRT, WaterSharedResources.WaterIntersectionHalfLineTensionMaskRT, waterContext.cmd, _rtScale);
            }
        }


        void ExecuteBackfacePrepass(WaterPass.WaterPassContext waterContext)
        {
            InitializeBackfaceTextures();
            waterContext.cmd.SetGlobalVector(MaskPassID.KWS_WaterBackfacePrePass_RTHandleScale, WaterSharedResources.WaterBackfacePrePassRT0.rtHandleProperties.rtHandleScale);

           
            CoreUtils.SetRenderTarget(waterContext.cmd, KWS_CoreUtils.GetMrt(WaterSharedResources.WaterBackfacePrePassRT0, WaterSharedResources.WaterBackfacePrePassRT1), WaterSharedResources.WaterBackfaceDepthRT, 
                                      ClearFlag.All, Color.clear);
            foreach (var waterInstance in WaterSharedResources.WaterInstances)
            {
                if (IsRequireBackfacePrePass(waterInstance))
                {
                    ExecuteInstance(waterContext.cam, waterContext.cmd, waterInstance, isBackface: true);
                }
            }
        }


        //void ExecuteVolumeComputeBuffer(WaterPass.WaterPassContext waterContext)
        //{
        //    if (WaterSharedResources.VolumeMasks.Count == 0) return;
        //    if (_volumeMaskMaterial                    == null) _volumeMaskMaterial = KWS_CoreUtils.CreateMaterial(KWS_ShaderConstants.ShaderNames.ClipMaskShaderName);


        //    _volumeMaskBufferCPU.Clear();
        //    uint instanceIndex = 0;
        //    foreach (var volumeMask in WaterSharedResources.VolumeMasks)
        //    {
        //        volumeMask._volumeData.InstanceID = instanceIndex + 1;
        //        _volumeMaskBufferCPU.Add(volumeMask._volumeData);

        //        instanceIndex++;
        //    }

        //    if (_volumeMaskBufferCPU.Count > 0)
        //    {
        //        _volumeMaskBufferGPU = KWS_CoreUtils.GetOrUpdateBuffer<KWS_WaterVolumeVariables.WaterVolumeData>(ref _volumeMaskBufferGPU, _volumeMaskBufferCPU.Count);
        //        _volumeMaskBufferGPU.SetData(_volumeMaskBufferCPU);
        //        Shader.SetGlobalInteger(PrePass.KWS_WaterVolumeDataBufferLength, _volumeMaskBufferCPU.Count);
        //        Shader.SetGlobalBuffer(PrePass.KWS_WaterVolumeDataBuffer, _volumeMaskBufferGPU);
        //    }
        //}



        //void ExectuteUnderwaterPrepass(WaterPass.WaterPassContext waterContext)
        //{
        //    _pyramidBlur.ComputeBlurPyramid(2.1f, WaterSharedResources.WaterPrePassRT0, WaterSharedResources.WaterMaskRTBlured, waterContext.cmd, _rtScale);
        //}

        //void ExecuteVolumeDepth(WaterPass.WaterPassContext waterContext)
        //{
        //    if (WaterSharedResources.VolumeMasks.Count == 0) return;

        //    if (WaterSharedResources.WaterVolumeDepthRT == null) InitializeClipMask();
        //    if (_volumeMaskMaterial                     == null) _volumeMaskMaterial = KWS_CoreUtils.CreateMaterial(KWS_ShaderConstants.ShaderNames.ClipMaskShaderName);

        //    OnSetRenderTargetVolumeMask?.Invoke(waterContext, WaterSharedResources.WaterVolumeDepthRT, WaterSharedResources.WaterVolumeIdRT);
        //    waterContext.cmd.BlitTriangle(_volumeMaskMaterial, pass: 1);

        //    /*
        //    CoreUtils.SetRenderTarget(waterContext.cmd, _jumpFloodTempRT[0], ClearFlag.Color, Color.clear);
        //    CoreUtils.SetRenderTarget(waterContext.cmd, _jumpFloodTempRT[1], ClearFlag.Color, Color.clear);

        //   // var currentSize = _jumpFloodTempRT[0].GetScaledSize(_jumpFloodTempRT[0].rtHandleProperties.currentViewportSize);
        //    int stepAmount = (int)Mathf.Log(_jumpFloodTempRT[0].rt.height, 2) + 1;

        //    var source      = WaterSharedResources.WaterMaskRT;
        //    var dest        = _jumpFloodTempRT[0];

        //    for (int i = 0; i < stepAmount; i++)
        //    {
        //        var step   = (int)Mathf.Pow(2, stepAmount - i - 1);
        //        waterContext.cmd.SetGlobalInteger("KWS_IsFirstPass", i == 0 ? 1 : 0);
        //        waterContext.cmd.SetGlobalInteger("_JumpFloodStep",  step);
        //        waterContext.cmd.BlitTriangle(source, dest, _volumeMaskMaterial, pass: 3);

        //        source = (float)i % 2 == 0 ? _jumpFloodTempRT[0] : _jumpFloodTempRT[1];
        //        dest = (float)i % 2 == 0 ? _jumpFloodTempRT[1] : _jumpFloodTempRT[0];
        //    }
        //    waterContext.cmd.SetGlobalTexture("KWS_JumpFloodVolumeMask", source);
        //    */



        //}

        void ExecuteInstance(Camera cam, CommandBuffer cmd, WaterSystem waterInstance, bool isBackface)
        {
            UpdateMaterialParams(cmd, waterInstance);
            var shaderPass             = waterInstance.CanRenderTesselation ? 1 : 0;
            if (isBackface) shaderPass += 2;
            var mat                    = waterInstance.InstanceData.MaterialWaterPrePass;

            switch (waterInstance.Settings.WaterMeshType)
            {
                case WaterMeshTypeEnum.InfiniteOcean:
                case WaterMeshTypeEnum.FiniteBox:
                    DrawInstancedQuadTree(cam, cmd, waterInstance, mat, shaderPass);
                    break;
                case WaterMeshTypeEnum.CustomMesh:
                    DrawCustomMesh(cmd, waterInstance, mat, shaderPass);
                    break;
                case WaterMeshTypeEnum.River:
                    DrawRiverSpline(cmd, waterInstance, mat, shaderPass);
                    break;
            }

        }

        public static void DrawInstancedQuadTree(Camera cam, CommandBuffer cmd, WaterSystem waterInstance, Material mat, int shaderPass)
        {
            //var isFastMode = !waterInstance.IsCameraUnderwaterForInstance;
            var isFastMode = false;
            if (!waterInstance._meshQuadTree.TryGetRenderingContext(cam, isFastMode, out var context)) return;
           
            if (waterInstance.Settings.WaterMeshType == WaterMeshTypeEnum.FiniteBox)
            {
                var     size = waterInstance.WaterSize;
                var pos  = waterInstance.WaterPivotWorldPosition;
                pos.y += 0.001f;
                cmd.SetKeyword(WaterKeywords.USE_WATER_INSTANCING, false);
                var matrix = Matrix4x4.TRS(pos, waterInstance.WaterPivotWorldRotation, size);
                cmd.DrawMesh(context.underwaterMesh, matrix, mat, 0, shaderPass);
            }

            if (context.chunkInstance == null || mat == null || context.visibleChunksArgs == null)
            {
                Debug.LogError($"Water PrePass.DrawInstancedQuadTree error: {context.chunkInstance}, { mat},  { context.visibleChunksArgs}");
                return;
            }

            cmd.SetKeyword(WaterKeywords.USE_WATER_INSTANCING, true);
            cmd.SetGlobalBuffer(StructuredBuffers.InstancedMeshData, context.visibleChunksComputeBuffer);
            cmd.DrawMeshInstancedIndirect(context.chunkInstance, 0, mat, shaderPass, context.visibleChunksArgs);
            
            cmd.SetKeyword(WaterKeywords.USE_WATER_INSTANCING, false);
        }

        private void DrawCustomMesh(CommandBuffer cmd, WaterSystem waterInstance, Material mat, int shaderPass)
        {
            var mesh = waterInstance.Settings.CustomMesh;
            if (mesh == null) return;
            var matrix = Matrix4x4.TRS(waterInstance.WaterPivotWorldPosition, waterInstance.WaterPivotWorldRotation, waterInstance.WaterSize);

            if (mat == null)
            {
                Debug.LogError($"Water PrePass.DrawCustomMesh error: { mat}");
                return;
            }
            cmd.DrawMesh(mesh, matrix, mat, 0, shaderPass);
        }

        private void DrawRiverSpline(CommandBuffer cmd, WaterSystem waterInstance, Material mat, int shaderPass)
        {
            var mesh = waterInstance.SplineRiverMesh;
            if (mesh == null) return;
            var matrix     = Matrix4x4.TRS(waterInstance.WaterPivotWorldPosition, Quaternion.identity, Vector3.one);

            if (mat == null)
            {
                Debug.LogError($"Water PrePass.DrawRiverSpline error: { mat}");
                return;
            }
            cmd.DrawMesh(mesh, matrix, mat, 0, shaderPass);
        }

        bool IsRequireRenderPrePass(WaterSystem waterInstance)
        {
            if (!IsWaterVisibleAndActive(waterInstance)) return false;
            //if (waterInstance.Settings.EnabledMeshRendering == false && waterInstance.Settings.UseCausticEffect == false) return false;
            //if (waterInstance.Settings.CanUseAquariumMode) return false;

            return true;
        }

        bool IsRequireBackfacePrePass(WaterSystem waterInstance)
        {
            if (!IsWaterVisibleAndActive(waterInstance)) return false;
            //if (waterInstance.Settings.EnabledMeshRendering == false && waterInstance.Settings.UseCausticEffect == false) return false;

            return waterInstance.Settings.CanUseAquariumMode;
        }

        private void UpdateMaterialParams(CommandBuffer cmd, WaterSystem waterInstance)
        {
            var settings = waterInstance.Settings;

            //cmd.SetGlobalVector(DynamicWaterParams.KW_WaterPosition, waterInstance.WaterRelativeWorldPosition);
            //cmd.SetGlobalFloat(DynamicWaterParams.KW_Time, KW_Extensions.TotalTime());
            cmd.SetGlobalFloat(DynamicWaterParams.KWS_ScaledTime, KW_Extensions.TotalTime() * settings.CurrentTimeScale);
            //cmd.SetGlobalInt(DynamicWaterParams.KWS_WaterInstanceID, waterInstance.WaterShaderPassID);

            waterInstance.InstanceData.UpdateDynamicShaderParams(cmd, WaterInstanceResources.DynamicPassEnum.FFT);


            if (settings.UseFluidsSimulation) waterInstance.InstanceData.UpdateDynamicShaderParams(cmd, WaterInstanceResources.DynamicPassEnum.FlowMap);
            //cmd.SetKeyword(WaterKeywords.USE_WATER_INSTANCING, waterInstance.Settings.WaterMeshType == WaterSystemScriptableData.WaterMeshTypeEnum.InfiniteOcean
            //                                                || waterInstance.Settings.WaterMeshType == WaterSystemScriptableData.WaterMeshTypeEnum.FiniteBox);

        }
    }
}