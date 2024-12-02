using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using static KWS.KWS_CoreUtils;
using static KWS.KWS_ShaderConstants;
using static KWS.WaterSystemScriptableData;

namespace KWS
{
    internal class CausticPrePass: WaterPass
    {
        internal override string PassName => "Water.CausticPrePass";

        private Dictionary<int, Mesh> _causticMeshes = new Dictionary<int, Mesh>();

        Material _causticMaterial;

        const float KWS_CAUSTIC_MULTIPLIER = 0.15f;

        Dictionary<CausticTextureResolutionQualityEnum, int> _causticQualityToMeshQuality = new Dictionary<CausticTextureResolutionQualityEnum, int>()
        {
            {CausticTextureResolutionQualityEnum.Ultra, 512},
            {CausticTextureResolutionQualityEnum.High, 384},
            {CausticTextureResolutionQualityEnum.Medium, 256},
            {CausticTextureResolutionQualityEnum.Low, 128},
        };


     
        public CausticPrePass()
        {
            WaterSharedResources.OnAnyWaterSettingsChanged += OnAnyWaterSettingsChanged;
            _causticMaterial                               =  KWS_CoreUtils.CreateMaterial(KWS_ShaderConstants.ShaderNames.CausticComputeShaderName);
        }

        void InitializeTextures()
        {
            var size = WaterSharedResources.MaxCausticResolution;
            var slices = WaterSharedResources.MaxCausticArraySlices;
            WaterSharedResources.CausticRTArray = KWS_CoreUtils.RTHandles.Alloc(size, size, colorFormat: GraphicsFormat.R8_UNorm, name: "_CausticRTArray", useMipMap: true, autoGenerateMips: false, slices: slices, dimension:TextureDimension.Tex2DArray);
            Shader.SetGlobalTexture(CausticID.KWS_CausticRTArray, WaterSharedResources.CausticRTArray);

            KWS_CoreUtils.ClearRenderTexture(WaterSharedResources.CausticRTArray.rt, ClearFlag.Color, new Color(KWS_CAUSTIC_MULTIPLIER, KWS_CAUSTIC_MULTIPLIER, KWS_CAUSTIC_MULTIPLIER));
            this.WaterLog(WaterSharedResources.CausticRTArray);
        }

        void ReleaseTextures()
        {
            WaterSharedResources.CausticRTArray?.Release();
            WaterSharedResources.CausticRTArray = null;
            this.WaterLog(string.Empty, KW_Extensions.WaterLogMessageType.ReleaseRT);
        }

        public override void Release()
        {
            WaterSharedResources.OnAnyWaterSettingsChanged -= OnAnyWaterSettingsChanged;
           
            ReleaseTextures();
            KW_Extensions.SafeDestroy(_causticMaterial);

            foreach (var causticMesh in _causticMeshes) KW_Extensions.SafeDestroy(causticMesh.Value);
            _causticMeshes.Clear();

            this.WaterLog(string.Empty, KW_Extensions.WaterLogMessageType.Release);
        }

        private void OnAnyWaterSettingsChanged(WaterSystem instance, WaterSystem.WaterTab changedTabs)
        {
            if(!WaterSharedResources.IsAnyWaterUseCaustic) return;

            if (changedTabs.HasTab(WaterSystem.WaterTab.Caustic))
            {
                if (WaterSharedResources.CausticRTArray == null 
                 || WaterSharedResources.CausticRTArray.rt.width != WaterSharedResources.MaxCausticResolution 
                 || WaterSharedResources.CausticRTArray.rt.volumeDepth != WaterSharedResources.MaxCausticArraySlices)
                {
                    ReleaseTextures();
                    InitializeTextures();
                }
            }
        }

        public override void ExecuteCommandBuffer(WaterPass.WaterPassContext waterContext)
        {
            if (!WaterSharedResources.IsAnyWaterUseCaustic) return;

            if(WaterSharedResources.CausticRTArray == null) InitializeTextures();

            var currentCausticID = 0;
            if (WaterSharedResources.IsAnyWaterUseGlobalWind)
            {
                ComputeCaustic(waterContext, WaterSharedResources.GlobalWaterSystem, currentCausticID);
                currentCausticID++;
            }

            
            for (var i = 0; i < WaterSharedResources.WaterInstances.Count; i++)
            {
                var instance = WaterSharedResources.WaterInstances[i];
                if (!instance.Settings.UseGlobalWind && instance.Settings.UseCausticEffect && IsWaterVisibleAndActive(instance))
                {
                    WaterSharedResources.InstanceToCausticID[i + 1] = currentCausticID;
                    ComputeCaustic(waterContext, instance, currentCausticID);
                    currentCausticID++;
                }
                else
                {
                    WaterSharedResources.InstanceToCausticID[i + 1] = -1;
                }
            }

            if (WaterSharedResources.CausticRTArray != null) waterContext.cmd.GenerateMips(WaterSharedResources.CausticRTArray);

            Shader.SetGlobalFloatArray(CausticID.KWS_InstanceToCausticID, WaterSharedResources.InstanceToCausticID);
        }

        void ComputeCaustic(WaterPass.WaterPassContext waterContext, WaterSystem waterInstance, int causticID)
        {
            var cmd = waterContext.cmd;
            CoreUtils.SetRenderTarget(waterContext.cmd, WaterSharedResources.CausticRTArray, ClearFlag.Color, Color.black, depthSlice: causticID);

            cmd.SetGlobalFloat(CausticID.KWS_CausticDepthScale, waterInstance.Settings.GetCurrentCausticDepth);
            cmd.SetGlobalFloat(KWS_ShaderConstants.ConstantWaterParams.KWS_WindSpeed, waterInstance.Settings.CurrentWindSpeed);
            cmd.SetGlobalFloat(KWS_ShaderConstants.ConstantWaterParams.KWS_WavesCascades, waterInstance.Settings.CurrentFftWavesCascades);
            cmd.SetGlobalFloat(KWS_ShaderConstants.ConstantWaterParams.KWS_WavesAreaScale, waterInstance.Settings.CurrentWavesAreaScale);

            cmd.SetGlobalTexture(KWS_ShaderConstants.FFT.KWS_FftWavesDisplace, WaterSharedResources.GetFftWavesDisplacementTexture(waterInstance).GetSafeTexture());

            cmd.SetKeyword(KWS_ShaderConstants.CausticKeywords.USE_CAUSTIC_FILTERING, waterInstance.Settings.GetCurrentCausticHighQualityFiltering && (int)waterInstance.Settings.CurrentFftWavesQuality <= 64);

            var mesh = GetOrCreateCausticMesh(waterInstance.Settings.GetCurrentCausticTextureResolutionQuality);
            cmd.DrawMesh(mesh, Matrix4x4.identity, _causticMaterial);
        }


        Mesh GetOrCreateCausticMesh(CausticTextureResolutionQualityEnum quality)
        {
            var size = _causticQualityToMeshQuality[quality];
            if (!_causticMeshes.ContainsKey(size))
            {
                _causticMeshes.Add(size, MeshUtils.CreatePlaneMesh(size, 1.2f));
            }

            return _causticMeshes[size];
        }

    }
}