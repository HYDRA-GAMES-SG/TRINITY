using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using static KWS.KWS_ShaderConstants;

namespace KWS
{
    internal class DrawMeshPass: WaterPass
    {
        internal override string       PassName => "Water.DrawMeshPass";
        private RenderParams _renderParams = new RenderParams();


        public DrawMeshPass()
        {
        }

        public override void Release()
        {
            //KW_Extensions.WaterLog(this, "Release", KW_Extensions.WaterLogMessageType.Release);
        }


        public override void ExecuteBeforeCameraRendering(Camera cam)
        {
            foreach (var waterInstance in WaterSharedResources.WaterInstances)
            {
                if (cam == null) return;
                var settings = waterInstance.Settings;

                var mat = waterInstance.InstanceData.GetCurrentWaterMaterial();
                mat.SetFloat(DynamicWaterParams.KWS_ScaledTime, KW_Extensions.TotalTime() * settings.CurrentTimeScale);

                FixIncorrectProbesUnityBug(waterInstance, mat);
                UpdateMaterialParams(waterInstance, mat);

                _renderParams.camera               = cam;
                _renderParams.material             = mat;
                _renderParams.reflectionProbeUsage = ReflectionProbeUsage.BlendProbes;
                _renderParams.worldBounds          = waterInstance.WorldSpaceBounds;
                _renderParams.renderingLayerMask   = GraphicsSettings.defaultRenderingLayerMask;
                _renderParams.layer                = KWS_Settings.Water.WaterLayer;

                switch (settings.WaterMeshType)
                {
                    case WaterSystemScriptableData.WaterMeshTypeEnum.InfiniteOcean:
                    case WaterSystemScriptableData.WaterMeshTypeEnum.FiniteBox:
                        DrawInstancedQuadTree(cam, waterInstance, waterInstance.InstanceData.GetCurrentWaterMaterial(), false);
                        break;
                    case WaterSystemScriptableData.WaterMeshTypeEnum.CustomMesh:
                        DrawCustomMesh(waterInstance, cam, mat);
                        break;
                     case WaterSystemScriptableData.WaterMeshTypeEnum.River:
                        DrawRiverSpline(waterInstance, cam, mat);
                         break;
                }
            }
        }

        private int[] _specCube              = new[] { Shader.PropertyToID("KWS_SpecCube0"), Shader.PropertyToID("KWS_SpecCube1") };
        private int[] _specCube_HDR          = new[] { Shader.PropertyToID("KWS_SpecCube0_HDR"), Shader.PropertyToID("KWS_SpecCube1_HDR") };
        private int[] _specCubeBoxMin        = new[] { Shader.PropertyToID("KWS_SpecCube0_BoxMin"), Shader.PropertyToID("KWS_SpecCube1_BoxMin") };
        private int[] _specCubeBoxMax        = new[] { Shader.PropertyToID("KWS_SpecCube0_BoxMax"), Shader.PropertyToID("KWS_SpecCube1_BoxMax") };
        private int[] _specCubeProbePosition = new[] { Shader.PropertyToID("KWS_SpecCube0_ProbePosition"), Shader.PropertyToID("KWS_SpecCube1_ProbePosition") };

        void FixIncorrectProbesUnityBug(WaterSystem waterInstance, Material mat)
        {
            //by some reason unity can't provide correct unity_SpecCube0_BoxMin/unity_SpecCube0_BoxMax using graphics.drawmesh
            var probes    = waterInstance.GetAffectedReflectionProbes();
       
            for (var i = 0; i < 2; i++)
            {
                
                var     pos       = Vector3.zero;
                var     boxMin    = Vector3.zero;
                var     boxMax    = Vector3.zero;

                var weight        = 0f;
                var blendDistance = 0f;
                var boxProjection = 0f;
                
                if (i + 1 <= probes.Count && probes[i].weight > 0.0f)
                {
                    var probe = probes[i].probe;

                    pos    = probe.transform.position;
                    boxMin = pos + probe.center - (probe.size                * 0.5f);
                    boxMax = pos                + probe.center + (probe.size * 0.5f);

                    weight          = probes[i].weight;
                    blendDistance   = probe.blendDistance;
                    boxProjection   = probe.boxProjection ? 1 : 0;
                  
                    mat.SetTexture(_specCube[i], probe.texture);
                    mat.SetVector(_specCube_HDR[i], probe.textureHDRDecodeValues);
                }

                mat.SetVector(_specCubeBoxMin[i],        new Vector4(boxMin.x, boxMin.y, boxMin.z, weight));
                mat.SetVector(_specCubeBoxMax[i],        new Vector4(boxMax.x, boxMax.y, boxMax.z, blendDistance));
                mat.SetVector(_specCubeProbePosition[i], new Vector4(pos.x,    pos.y,    pos.z,    boxProjection));
                
               
            }
        }

        public void DrawInstancedQuadTree(Camera cam, WaterSystem waterInstance, Material mat, bool isPrePass)
        {
            waterInstance._meshQuadTree.UpdateQuadTree(cam, waterInstance);
            var isFastMode = isPrePass && !waterInstance.IsCameraUnderwaterForInstance;
            if (!waterInstance._meshQuadTree.TryGetRenderingContext(cam, isFastMode, out var context)) return;

            mat.SetBuffer(StructuredBuffers.InstancedMeshData, context.visibleChunksComputeBuffer);

            //Graphics.DrawMeshInstancedIndirect(context.chunkInstance, 0, mat, waterInstance.WorldSpaceBounds, context.visibleChunksArgs, camera: cam);
            if (_renderParams.camera == null || _renderParams.material == null || context.chunkInstance == null || context.visibleChunksArgs == null || context.visibleChunksArgs.count == 0)
            {
                Debug.LogError($"Water draw mesh rendering error: {_renderParams.camera}, { _renderParams.material}, {context.chunkInstance}, {context.visibleChunksArgs}");
                return;
            }

            Graphics.RenderMeshIndirect(_renderParams, context.chunkInstance, context.visibleChunksArgs);
        }

        
        private void DrawCustomMesh(WaterSystem waterInstance, Camera cam, Material mat)
        {
            var mesh = waterInstance.Settings.CustomMesh;
            if (mesh == null) return;
            var matrix = Matrix4x4.TRS(waterInstance.WaterPivotWorldPosition, waterInstance.WaterPivotWorldRotation, waterInstance.WaterSize);
            // Graphics.DrawMesh(mesh, matrix, mat, 0, camera: cam);

            if (_renderParams.camera == null || _renderParams.material == null)
            {
                Debug.LogError($"Water draw mesh rendering error: {_renderParams.camera}, { _renderParams.material}");
                return;
            }

            Graphics.RenderMesh(_renderParams, mesh, 0, matrix);
        }

        private void DrawRiverSpline(WaterSystem waterInstance, Camera cam, Material mat)
        {
            var mesh = waterInstance.SplineRiverMesh;
            if (mesh == null) return;
            var matrix        = Matrix4x4.TRS(waterInstance.WaterPivotWorldPosition, Quaternion.identity, Vector3.one);
            //Graphics.DrawMesh(mesh, matrix, mat, 0, camera: cam);

            if (_renderParams.camera == null || _renderParams.material == null)
            {
                Debug.LogError($"Water draw mesh rendering error: {_renderParams.camera}, { _renderParams.material}");
                return;
            }
            Graphics.RenderMesh(_renderParams, mesh, 0, matrix);
        }
            
        private void UpdateMaterialParams(WaterSystem waterInstance, Material mat)
        {
            var settings = waterInstance.Settings;
            waterInstance.InstanceData.UpdateDynamicShaderParams(mat, WaterInstanceResources.DynamicPassEnum.FFT);
            if (settings.UseFlowMap) waterInstance.InstanceData.UpdateDynamicShaderParams(mat,          WaterInstanceResources.DynamicPassEnum.FlowMap);
        }
    }
}