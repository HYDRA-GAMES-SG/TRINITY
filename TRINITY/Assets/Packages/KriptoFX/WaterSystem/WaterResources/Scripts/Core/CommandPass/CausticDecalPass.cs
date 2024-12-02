using System;
using System.Collections.Generic;
using UnityEngine;
using static KWS.KWS_CoreUtils;
using static KWS.KWS_ShaderConstants;
using static KWS.WaterSystemScriptableData;
using UnityEngine.Rendering;

namespace KWS
{
    internal class CausticDecalPass : WaterPass
    {
        internal override string PassName => "Water.CausticDecalPass";

        private           Mesh   _decalMesh;
        Material                 _decalMaterial;

        Dictionary<CausticTextureResolutionQualityEnum, float> _causticQualityToDispersionStrength = new Dictionary<CausticTextureResolutionQualityEnum, float>()
        {
            {CausticTextureResolutionQualityEnum.Ultra, 1.5f},
            {CausticTextureResolutionQualityEnum.High, 1.25f},
            {CausticTextureResolutionQualityEnum.Medium, 1.0f},
            {CausticTextureResolutionQualityEnum.Low, 0.75f},
        };


        public CausticDecalPass()
        {
            _decalMaterial = KWS_CoreUtils.CreateMaterial(KWS_ShaderConstants.ShaderNames.CausticDecalShaderName, useWaterStencilMask: true);
            _decalMesh = MeshUtils.CreateCubeMesh();
        }


        void ReleaseTextures()
        {
            WaterSharedResources.CausticRTArray?.Release();
            WaterSharedResources.CausticRTArray = null;
            this.WaterLog(string.Empty, KW_Extensions.WaterLogMessageType.ReleaseRT);
        }

        public override void Release()
        {
            KW_Extensions.SafeDestroy(_decalMesh, _decalMaterial);

            this.WaterLog(string.Empty, KW_Extensions.WaterLogMessageType.Release);
        }


        public override void ExecuteCommandBuffer(WaterPass.WaterPassContext waterContext)
        {
            if (!WaterSharedResources.IsAnyWaterUseCaustic) return;

            foreach (var instance in WaterSharedResources.WaterInstances)
            {
                if (instance.Settings.UseCausticEffect && IsWaterVisibleAndActive(instance))
                {
                    DrawCausticDecal(waterContext, instance);
                }
            }

            ResetKeywords(waterContext.cmd);
        }

        void DrawCausticDecal(WaterPass.WaterPassContext waterContext, WaterSystem waterInstance)
        {
            var cmd      = waterContext.cmd;
            var settings = waterInstance.Settings;

            cmd.SetGlobalVector(DynamicWaterParams.KW_WaterPosition, waterInstance.WaterPivotWorldPosition);
            cmd.SetGlobalFloat(CausticID.KWS_CaustisStrength, settings.CausticStrength);
            cmd.SetGlobalFloat(CausticID.KWS_CaustisDispersionStrength, _causticQualityToDispersionStrength[settings.GetCurrentCausticTextureResolutionQuality]);
            cmd.SetGlobalInt(DynamicWaterParams.KWS_WaterInstanceID, waterInstance.WaterShaderPassID);

            if (settings.UseFlowMap) waterInstance.InstanceData.UpdateDynamicShaderParams(cmd, WaterInstanceResources.DynamicPassEnum.FlowMap);

            SetKeywords(cmd, waterInstance);

            var decalSize = waterInstance.WorldSpaceBounds.size;
            var decalPos = waterInstance.WaterRelativeWorldPosition;

            if (settings.WaterMeshType == WaterMeshTypeEnum.InfiniteOcean)
            {
                var farDistance = waterContext.cam.farClipPlane;
                decalSize.x = Mathf.Min(decalSize.x, farDistance);
                decalSize.z = Mathf.Min(decalSize.z, farDistance);
                decalSize.y = KWS_Settings.Caustic.CausticDecalHeight;

                decalPos.y -= KWS_Settings.Caustic.CausticDecalHeight * 0.5f - 1;
            }
            else if (settings.WaterMeshType == WaterMeshTypeEnum.FiniteBox)
            {
                decalPos = waterInstance.WorldSpaceBounds.center;
            }
            else if (settings.WaterMeshType == WaterMeshTypeEnum.CustomMesh)
            {
                decalPos = waterInstance.WorldSpaceBounds.center;
            }

            var decalTRS = Matrix4x4.TRS(decalPos, Quaternion.identity, decalSize); //todo precompute trs matrix

            CoreUtils.SetRenderTarget(waterContext.cmd, waterContext.cameraColor);
            cmd.DrawMesh(_decalMesh, decalTRS, _decalMaterial);
        }

        void SetKeywords(CommandBuffer cmd, WaterSystem waterInstance)
        {
            var settings            = waterInstance.Settings;
            var isFlowmapUsed       = settings.UseFlowMap && !settings.UseFluidsSimulation;
            var isFlowmapFluidsUsed = settings.UseFlowMap && settings.UseFluidsSimulation;

            cmd.SetKeyword(WaterKeywords.USE_SHORELINE,      settings.UseShorelineRendering);
            cmd.SetKeyword(WaterKeywords.KW_DYNAMIC_WAVES,   settings.UseDynamicWaves);
            cmd.SetKeyword(WaterKeywords.KW_FLOW_MAP,        isFlowmapUsed);
            cmd.SetKeyword(WaterKeywords.KW_FLOW_MAP_FLUIDS, isFlowmapFluidsUsed);
            cmd.SetKeyword(CausticKeywords.USE_DISPERSION,   settings.UseCausticDispersion);
        }

        //by some reason unity can't reset these keywords correctly after scene reloading and material keywords always broken
        void ResetKeywords(CommandBuffer cmd)
        {
            cmd.DisableShaderKeyword(WaterKeywords.USE_SHORELINE);
            cmd.DisableShaderKeyword(WaterKeywords.KW_DYNAMIC_WAVES);
            cmd.DisableShaderKeyword(WaterKeywords.KW_FLOW_MAP);
            cmd.DisableShaderKeyword(WaterKeywords.KW_FLOW_MAP_FLUIDS);
            cmd.DisableShaderKeyword(CausticKeywords.USE_DISPERSION);
        }
    }
}