using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace KWS
{
    internal class WaterInstanceResources
    {
        internal WaterSystem WaterInstance;

        internal int BuoyancyLeftFramesAfterRequest = int.MaxValue;
       
        internal void Initialize(WaterSystem waterInstance)
        {
            WaterInstance = waterInstance;
        }

        internal void Release()
        {
            FftWavesTextures.Release();
           
            KW_Extensions.SafeDestroy(_materialWater, _materialWaterTesselated, MaterialWaterPrePass);
        }


        #region Materials
            
        internal Material MaterialWaterPrePass;

        Material _materialWater;
        Material _materialWaterTesselated;

        public Material GetCurrentWaterMaterial()
        { 
           var mat = WaterInstance.CanRenderTesselation ? _materialWaterTesselated : _materialWater;
           mat.renderQueue = KWS_Settings.Water.DefaultWaterQueue + WaterInstance.Settings.TransparentSortingPriority;
           return mat;
        } 


        internal void InitializeMaterials()
        {
            MaterialWaterPrePass = KWS_CoreUtils.CreateMaterial(KWS_ShaderConstants.ShaderNames.WaterPrePassShaderName, useWaterStencilMask: true);

            _materialWater = KWS_CoreUtils.CreateMaterial(KWS_ShaderConstants.ShaderNames.WaterShaderName, useWaterStencilMask: true);
            _materialWaterTesselated = KWS_CoreUtils.CreateMaterial(KWS_ShaderConstants.ShaderNames.WaterTesselatedShaderName, useWaterStencilMask: true);
           
           UpdateInstanceMaterialsConstantParams();
        }

        internal void UpdateInstanceMaterialsConstantParams()
        {
            WaterInstance.SetConstantShaderParamsShared(MaterialWaterPrePass, true);
            WaterInstance.SetConstantShaderParamsShared(GetCurrentWaterMaterial(), false);
        }
      
        internal void UpdateInstanceID()
        {
            WaterInstance.UpdateWaterInstanceID(MaterialWaterPrePass);
            WaterInstance.UpdateWaterInstanceID(GetCurrentWaterMaterial());
        }

        #endregion


        //////////////////////////////////////////////////////////////////////

        #region FFT

        internal        FftWavesPass.FftWavesData              FftWavesTextures = new FftWavesPass.FftWavesData();
        internal        AsyncTextureSynchronizer<FftGPUHeightData> FftGpuHeightAsyncData;
        internal        int                                        FftHeightDataTexSize;
        internal RTHandle                                   FftBuoyancyHeight;
        #endregion



        ///////////////////Reflection //////////////////////////////////////


        //////////////////////////////////////////////////////////////////////



        /////////////////// flowing /////////////////////////////////
        internal float   FluidsAreaSize;
        internal float   FluidsSpeed;
        internal Texture Flowmap;

        internal Vector3 FluidsAreaPositionLod0;
        internal Vector3 FluidsAreaPositionLod1;

        internal RenderTexture[] FluidsRT     = new RenderTexture[2];
        internal RenderTexture[] FluidsFoamRT = new RenderTexture[2];
        internal RenderTexture[] FluidsMask = new RenderTexture[2];
        ////////////////////////////////////////////////////////////////


        public enum DynamicPassEnum
        {
            FFT,
            PlanarReflection,
            FluidsSimulation,
            FlowMap,
        }

        public void UpdateDynamicShaderParams(Material mat, DynamicPassEnum pass)
        {
            switch (pass)
            {
                case DynamicPassEnum.FFT:
                    mat.SetFloat(KWS_ShaderConstants.ConstantWaterParams.KWS_WavesCascades,  WaterInstance.Settings.CurrentFftWavesCascades);
                    mat.SetFloat(KWS_ShaderConstants.ConstantWaterParams.KWS_WavesAreaScale, WaterInstance.Settings.CurrentWavesAreaScale);
                    mat.SetTexture(KWS_ShaderConstants.FFT.KWS_FftWavesDisplace, WaterSharedResources.GetFftWavesDisplacementTexture(WaterInstance).GetSafeTexture());
                    mat.SetTexture(KWS_ShaderConstants.FFT.KWS_FftWavesNormal,   WaterSharedResources.GetFftWavesNormalTexture(WaterInstance).GetSafeTexture());
                    mat.SetFloat(KWS_ShaderConstants.ConstantWaterParams.KW_WaterFarDistance, WaterInstance.Settings.OceanDetailingFarDistance);
                    break;

                //case DynamicPassEnum.PlanarReflection:
                //    mat.SetTexture(KWS_ShaderConstants.ReflectionsID.KWS_PlanarReflectionRT, WaterSharedResources.PlanarReflection.GetSafeTexture());
                //    break;


                case DynamicPassEnum.FlowMap:
                    var flowmap = Flowmap != null ? Flowmap : WaterInstance.Settings.FlowingScriptableData?.FlowmapTexture;
                    mat.SetTexture(KWS_ShaderConstants.FlowmapID.KW_FlowMapTex, flowmap.GetSafeTexture(Color.gray));
                    mat.SetVector(KWS_ShaderConstants.ConstantWaterParams.KW_FlowMapOffset, WaterInstance.Settings.FlowMapAreaPosition);
                    mat.SetFloat(KWS_ShaderConstants.ConstantWaterParams.KW_FlowMapSize,  WaterInstance.Settings.FlowMapAreaSize);
                    mat.SetFloat(KWS_ShaderConstants.ConstantWaterParams.KW_FlowMapSpeed, WaterInstance.Settings.FlowMapSpeed);
                    break;

                case DynamicPassEnum.FluidsSimulation:
                    mat.SetFloat(KWS_ShaderConstants.FlowmapID.KW_FluidsVelocityAreaScale, (0.5f * FluidsAreaSize / 40f) * FluidsSpeed);
                    mat.SetFloat(KWS_ShaderConstants.FlowmapID.KW_FluidsMapAreaSize_lod0,  FluidsAreaSize);
                    mat.SetFloat(KWS_ShaderConstants.FlowmapID.KW_FluidsMapAreaSize_lod1,  FluidsAreaSize * KWS_Settings.Flowing.AreaSizeMultiplierLod1);
                    //mat.SetVector(KWS_ShaderConstants.FlowmapID.KW_FluidsMapWorldPosition_lod0, FluidsAreaPositionLod0);
                    //mat.SetVector(KWS_ShaderConstants.FlowmapID.KW_FluidsMapWorldPosition_lod1, FluidsAreaPositionLod1);
                    mat.SetTexture(KWS_ShaderConstants.FlowmapID.KW_Fluids_Lod0,      FluidsRT[0].GetSafeTexture());
                    mat.SetTexture(KWS_ShaderConstants.FlowmapID.KW_Fluids_Lod1,      FluidsRT[1].GetSafeTexture());
                    mat.SetTexture(KWS_ShaderConstants.FlowmapID.KWS_FluidsMask_Lod0, FluidsMask[0].GetSafeTexture());
                    mat.SetTexture(KWS_ShaderConstants.FlowmapID.KWS_FluidsMask_Lod1, FluidsFoamRT[1].GetSafeTexture());

                    break;

                    //default:
                    //    throw new ArgumentOutOfRangeException(nameof(pass), pass, null);
            }

        }

        public void UpdateDynamicShaderParams(CommandBuffer cmd, DynamicPassEnum pass)
        {
            switch (pass)
            {
                case DynamicPassEnum.FFT:
                    cmd.SetGlobalFloat(KWS_ShaderConstants.ConstantWaterParams.KWS_WavesCascades,  WaterInstance.Settings.CurrentFftWavesCascades);
                    cmd.SetGlobalFloat(KWS_ShaderConstants.ConstantWaterParams.KWS_WavesAreaScale, WaterInstance.Settings.CurrentWavesAreaScale);
                    cmd.SetGlobalTexture(KWS_ShaderConstants.FFT.KWS_FftWavesDisplace, WaterSharedResources.GetFftWavesDisplacementTexture(WaterInstance).GetSafeTexture());
                    cmd.SetGlobalTexture(KWS_ShaderConstants.FFT.KWS_FftWavesNormal,   WaterSharedResources.GetFftWavesNormalTexture(WaterInstance).GetSafeTexture());
                    cmd.SetGlobalFloat(KWS_ShaderConstants.ConstantWaterParams.KW_WaterFarDistance, WaterInstance.Settings.OceanDetailingFarDistance);
                    break;
              
                case DynamicPassEnum.PlanarReflection:
                    cmd.SetGlobalTexture(KWS_ShaderConstants.ReflectionsID.KWS_PlanarReflectionRT, WaterSharedResources.PlanarReflection.GetSafeTexture());
                    break;


                case DynamicPassEnum.FlowMap:
                    var flowmap = Flowmap != null ? Flowmap : WaterInstance.Settings.FlowingScriptableData?.FlowmapTexture;
                    cmd.SetGlobalTexture(KWS_ShaderConstants.FlowmapID.KW_FlowMapTex, flowmap.GetSafeTexture(Color.gray));
                    cmd.SetGlobalVector(KWS_ShaderConstants.ConstantWaterParams.KW_FlowMapOffset, WaterInstance.Settings.FlowMapAreaPosition);
                    cmd.SetGlobalFloat(KWS_ShaderConstants.ConstantWaterParams.KW_FlowMapSize,  WaterInstance.Settings.FlowMapAreaSize);
                    cmd.SetGlobalFloat(KWS_ShaderConstants.ConstantWaterParams.KW_FlowMapSpeed, WaterInstance.Settings.FlowMapSpeed);
                    break;

                case DynamicPassEnum.FluidsSimulation:
                    cmd.SetGlobalFloat(KWS_ShaderConstants.FlowmapID.KW_FluidsVelocityAreaScale, (0.5f * FluidsAreaSize / 40f) * FluidsSpeed);
                    cmd.SetGlobalFloat(KWS_ShaderConstants.FlowmapID.KW_FluidsMapAreaSize_lod0,  FluidsAreaSize);
                    cmd.SetGlobalFloat(KWS_ShaderConstants.FlowmapID.KW_FluidsMapAreaSize_lod1,  FluidsAreaSize * KWS_Settings.Flowing.AreaSizeMultiplierLod1);
                    cmd.SetGlobalVector(KWS_ShaderConstants.FlowmapID.KW_FluidsMapWorldPosition_lod0, FluidsAreaPositionLod0);
                    cmd.SetGlobalVector(KWS_ShaderConstants.FlowmapID.KW_FluidsMapWorldPosition_lod1, FluidsAreaPositionLod1);
                    cmd.SetGlobalTexture(KWS_ShaderConstants.FlowmapID.KW_Fluids_Lod0,      FluidsRT[0].GetSafeTexture());
                    cmd.SetGlobalTexture(KWS_ShaderConstants.FlowmapID.KW_Fluids_Lod1,      FluidsRT[1].GetSafeTexture());
                    cmd.SetGlobalTexture(KWS_ShaderConstants.FlowmapID.KW_FluidsFoam_Lod0,  FluidsFoamRT[0].GetSafeTexture());
                    cmd.SetGlobalTexture(KWS_ShaderConstants.FlowmapID.KW_FluidsFoam_Lod1,  FluidsFoamRT[1].GetSafeTexture());
                    cmd.SetGlobalTexture(KWS_ShaderConstants.FlowmapID.KWS_FluidsMask_Lod0, FluidsMask[0].GetSafeTexture());
                    cmd.SetGlobalTexture(KWS_ShaderConstants.FlowmapID.KWS_FluidsMask_Lod1, FluidsMask[1].GetSafeTexture());

                    break;

                //default:
                //    throw new ArgumentOutOfRangeException(nameof(pass), pass, null);
            }

        }

    }
}

