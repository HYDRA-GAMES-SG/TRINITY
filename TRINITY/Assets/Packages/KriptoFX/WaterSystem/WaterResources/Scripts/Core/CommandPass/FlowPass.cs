using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;


namespace KWS
{
    internal class FlowPass: WaterPass
    {
        internal override string        PassName => "Water.FlowPass";
        private           CommandBuffer _cmd;

        private int _passBaking    = 0;
        private int _passCompute   = 1;
        private int _passRemap     = 2;
        private int _passDepthMask = 3;

        public FlowPass()
        {
            WaterSharedResources.OnAnyWaterSettingsChanged += OnAnyWaterSettingsChanged;
        }

        public override void Release()
        {
            WaterSharedResources.OnAnyWaterSettingsChanged -= OnAnyWaterSettingsChanged;

            ReleaseFluidsResources();

            this.WaterLog(string.Empty, KW_Extensions.WaterLogMessageType.Release);
        }

        private void OnAnyWaterSettingsChanged(WaterSystem instance, WaterSystem.WaterTab changedTabs)
        {
            if (changedTabs.HasTab(WaterSystem.WaterTab.Flow))
            {
                
            }
        }

        public override void ExecutePerFrame(HashSet<Camera> cameras, CustomFixedUpdates fixedUpdates)
        {
            if (fixedUpdates.FramesCount_60fps == 0) return;
            if (!WaterSharedResources.IsAnyWaterUseFluidsFoam) return;
            
           
            var cam = KWS_CoreUtils.GetFixedUpdateCamera(cameras);
            if (cam  == null) return;

            if (_cmd == null) _cmd = new CommandBuffer() { name = PassName };
            _cmd.Clear();
            var requireExecuteCmd = false;

            foreach (var waterInstance in WaterSharedResources.WaterInstances)
            {
                waterInstance.IsFluidsRenderedInTheCurrentFrame = false;
                if (fluidsData[0] != null && waterInstance.SplineRiverMesh != null)
                {
                    CoreUtils.SetRenderTarget(_cmd, fluidsData[0].DepthStencilMask);
                    RenderDepthPrepass(cam, _cmd, waterInstance, fluidsData[0].DepthStencilMask, waterInstance.Settings.FluidsAreaSize * KWS_Settings.Flowing.AreaSizeMultiplierLod1, cam.GetCameraPositionFast());

                    CoreUtils.SetRenderTarget(_cmd, fluidsData[2].DepthStencilMask);
                    RenderDepthPrepass(cam, _cmd, waterInstance, fluidsData[2].DepthStencilMask, waterInstance.Settings.FluidsAreaSize , cam.GetCameraPositionFast());
                }
            }

            foreach (var waterInstance in WaterSharedResources.WaterInstances)
            {
                if (waterInstance.Settings.UseFlowMap 
                 && waterInstance.Settings.UseFluidsSimulation 
                 && !waterInstance._isFluidsSimBakedMode 
                 && !waterInstance.IsFluidsRenderedInTheCurrentFrame
                 && waterInstance.SplineRiverMesh != null)
                {
                    var iterations = fixedUpdates.FramesCount_60fps * waterInstance.Settings.FluidsSimulationIterrations;
                    for (int i = 0; i <= iterations; i++)
                    {
                        RenderFluids(cam, _cmd, waterInstance);
                        requireExecuteCmd = true;
                    }

                    waterInstance.IsFluidsRenderedInTheCurrentFrame = true;
                }

                if (waterInstance._isFluidsSimBakedMode)
                {
                    BakeFluidSimulationFrame(_cmd, waterInstance);
                    requireExecuteCmd = true;
                }
            }

            if (requireExecuteCmd) Graphics.ExecuteCommandBuffer(_cmd);
        }

      
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        FluidsData[] fluidsData = new FluidsData[4];

      
        Texture2D       foamTex;
        RTHandle        prebakedFluidsRT;

        Material fluidsMaterial;
        private Vector3? lastPosition = null;
        private Vector3? lastPosition_lod1;
        private int frameNumber;
        int lastWidth;
        int lastHeight;
        float currentJitterOffsetTime;

        private const string fluidSimulationShaderName = "Hidden/KriptoFX/KWS/FluidSimulation";


        private int ID_KW_FluidsDepth = Shader.PropertyToID("KW_FluidsDepthTex");
        private int ID_KW_FluidsDepthOrthoSize = Shader.PropertyToID("KW_FluidsDepthOrthoSize");
        private int ID_KW_FluidsDepthNearFarDistance = Shader.PropertyToID("KW_FluidsDepth_Near_Far_Dist");
        private int ID_KW_FluidsDepthPos = Shader.PropertyToID("KW_FluidsDepthPos");

        private const int nearPlaneDepth = -2;
        private const int farPlaneDepth = 100;


        class FluidsData
        {
            public RTHandle DataRT;
            public RTHandle FoamRT; 
            public RTHandle DepthStencilMask;

        }

        void InitializeTextures(int width, int height)
        {
            for (int i = 0; i < 4; i++)
            {
                if (fluidsData[i] == null) fluidsData[i] = new FluidsData();

                fluidsData[i].DataRT           = KWS_CoreUtils.RTHandles.Alloc(width, height, name: "fluidsDataRT",           colorFormat: GraphicsFormat.R16G16B16A16_SFloat);
                fluidsData[i].FoamRT           = KWS_CoreUtils.RTHandles.Alloc(width, height, name: "fluidsFoamRT",           colorFormat: GraphicsFormat.R8_UNorm);
                fluidsData[i].DepthStencilMask = KWS_CoreUtils.RTHandles.Alloc(width, height, name: "fluidsDepthStencilMask", depthBufferBits: DepthBits.Depth8);
            }

            lastWidth   = width;
            lastHeight  = height;
            frameNumber = 0;
        }

        void SetShaderParams(CommandBuffer cmd, WaterSystem waterInstance)
        {
            var flowingData = waterInstance.Settings.FlowingScriptableData;
            if (flowingData == null) return;

            if (flowingData.FlowmapTexture != null)
            {
                cmd.SetGlobalVector(KWS_ShaderConstants.ConstantWaterParams.KW_FlowMapOffset, waterInstance.Settings.FlowMapAreaPosition);
                cmd.SetGlobalFloat(KWS_ShaderConstants.ConstantWaterParams.KW_FlowMapSize, waterInstance.Settings.FlowMapAreaSize);
                cmd.SetGlobalTexture(KWS_ShaderConstants.FlowmapID.KW_FlowMapTex, waterInstance.Settings.FlowingScriptableData.FlowmapTexture.GetSafeTexture(Color.gray));
            }

            if (flowingData.FluidsMaskTexture != null)
            {
                cmd.SetGlobalTexture(ID_KW_FluidsDepth, flowingData.FluidsMaskTexture);
            }

            if (flowingData.FluidsPrebakedTexture != null)
            {
                cmd.SetGlobalTexture("KW_FluidsPrebaked", flowingData.FluidsPrebakedTexture);
            }
        }

        //private void LoadDepthTexture(WaterSystem waterInstance)
        //{
        //    var flowingData = waterInstance.Settings.FlowingScriptableData;
        //    if (flowingData == null || flowingData.FluidsMaskTexture == null || flowingData.FluidsPrebakedTexture == null) return;

        //    waterInstance.SetTextures(false, (ID_KW_FluidsDepth, flowingData.FluidsMaskTexture));
        //    waterInstance.SetFloats(false, (ID_KW_FluidsDepthOrthoSize, flowingData.AreaSize));
        //    waterInstance.SetVectors(false, (ID_KW_FluidsDepthNearFarDistance, new Vector3(nearPlaneDepth, farPlaneDepth, farPlaneDepth - nearPlaneDepth)));
        //    waterInstance.SetVectors(false, (ID_KW_FluidsDepthPos, flowingData.AreaPosition));
        //}

        public void ReleaseFluidsResources()
        {
            for (int i = 0; i < 4; i++)
            {
                if (fluidsData[i] != null)
                {
                    fluidsData[i].DataRT?.Release();
                    fluidsData[i].FoamRT?.Release();
                    fluidsData[i].DepthStencilMask?.Release();
                }
            }
      

            KW_Extensions.SafeDestroy(fluidsMaterial);
            Resources.UnloadAsset(foamTex);
            foamTex = null;
            lastPosition = null;
            lastWidth = 0;
            lastHeight = 0;
            frameNumber = 0;

        }

        public void InitializeMaterials(WaterSystem waterInstance)
        {
            if (fluidsMaterial == null)
            {
                fluidsMaterial = KWS_CoreUtils.CreateMaterial(fluidSimulationShaderName);
            }

        }

        Vector3 ComputeAreaSimulationJitter(float offsetX, float offsetZ)
        {
            var time      = KW_Extensions.TotalTime()*10;
            var jitterSin = Mathf.Sin(time);
            var jitterCos = Mathf.Cos(time);
            var jitter    = new Vector3(offsetX * jitterSin, 0, offsetZ * jitterCos) * 2;
            //currentJitterOffsetTime += 12.0f;
            return jitter;
        }

        Vector3 RayToWaterPos(Camera currentCamera, float height)
        {
            var ray = currentCamera.ViewportPointToRay(new Vector3(0.5f, 0.0f, 0));
            var plane = new Plane(Vector3.down, height);
            float distanceToPlane;
            if (plane.Raycast(ray, out distanceToPlane))
            {
                return ray.GetPoint(distanceToPlane);
            }
            return currentCamera.transform.position;
        }

        bool CheckIfFlowmapDataContains(WaterSystem waterInstance)
        {
            if (waterInstance.Settings.FlowingScriptableData == null)
            {
                Debug.LogError("You should draw a flow map before baking the simulation! Use 'Water->Flowing->Flowmap painter'");
                return false;
            }

            return true;
        }

        public void SaveOrthoDepth(WaterSystem waterInstance, Vector3 position, int areaSize, int texSize)
        {
#if UNITY_EDITOR
            if (!CheckIfFlowmapDataContains(waterInstance)) return;

            var pathToSceneFolder = KW_Extensions.GetPathToSceneInstanceFolder();
            var pathToFile        = Path.Combine(pathToSceneFolder, KWS_Settings.DataPaths.FluidsMaskTexture);


            texSize = Mathf.Clamp(texSize, 512, 2048);
            var depthTex = MeshUtils.GetRiverOrthoDepth(waterInstance, position, areaSize, texSize, nearPlaneDepth, farPlaneDepth);
            depthTex.SaveTexture(pathToFile, useAutomaticCompressionFormat: true, KW_Extensions.UsedChannels._R, isHDR: false, mipChain: false);

            waterInstance.Settings.FlowingScriptableData.FluidsMaskTexture = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(pathToFile + ".kwsTexture");
            UnityEditor.EditorUtility.SetDirty(waterInstance.Settings.FlowingScriptableData);

            KW_Extensions.SafeDestroy(depthTex);
#else
            Debug.LogError("You can't save ortho depth data in runtime");
            return;
#endif

        }

        private int KW_FluidsVelocityAreaScale = Shader.PropertyToID("KW_FluidsVelocityAreaScale");
        private int KW_FluidsMapWorldPosition_lod0 = Shader.PropertyToID("KW_FluidsMapWorldPosition_lod0");
        private int KW_FluidsMapWorldPosition_lod1 = Shader.PropertyToID("KW_FluidsMapWorldPosition_lod1");
        private int KW_FluidsMapAreaSize_lod0 = Shader.PropertyToID("KW_FluidsMapAreaSize_lod0");
        private int KW_FluidsMapAreaSize_lod1 = Shader.PropertyToID("KW_FluidsMapAreaSize_lod1");
        int KW_Fluids_Lod0 = Shader.PropertyToID("KW_Fluids_Lod0");
        int KW_FluidsFoam_Lod0 = Shader.PropertyToID("KW_FluidsFoam_Lod0");
        int KW_Fluids_Lod1 = Shader.PropertyToID("KW_Fluids_Lod1");
        int KW_FluidsFoam_Lod1 = Shader.PropertyToID("KW_FluidsFoam_Lod1");
        private int KW_FluidsRequiredReadPrebakedSim = Shader.PropertyToID("KW_FluidsRequiredReadPrebakedSim");

        string KW_FLUIDS_PREBAKE_SIM = "KW_FLUIDS_PREBAKE_SIM";


        void BakeFluidSimulationFrame(CommandBuffer cmd, WaterSystem waterInstance)
        {
            if (waterInstance.currentBakeFluidsFrames < waterInstance.BakeFluidsLimitFrames)
            {
                if (waterInstance.currentBakeFluidsFrames == 0)
                {
                    SaveOrthoDepth(waterInstance, waterInstance.Settings.FlowMapAreaPosition, waterInstance.Settings.FlowMapAreaSize, (int)waterInstance.Settings.FlowMapTextureResolution);
                }
                waterInstance.currentBakeFluidsFrames++;
                waterInstance.BakedFluidsSimPercentPassed = (int)((100f / waterInstance.BakeFluidsLimitFrames) * waterInstance.currentBakeFluidsFrames);
                for (int j = 0; j < 10; j++)
                    PrebakeSimulation(cmd, waterInstance, waterInstance.Settings.FlowMapAreaPosition, waterInstance.Settings.FlowMapAreaSize, 4096, waterInstance.Settings.FluidsSpeed, 0.1f);
            }
            else if (waterInstance._isFluidsSimBakedMode)
            {
                SavePrebakedSimulation(waterInstance);
                waterInstance.BakedFluidsSimPercentPassed = 0;
                frameNumber                               = 0;
                waterInstance._isFluidsSimBakedMode       = false;
                Debug.Log("Fluids obstacles saved!");
            }
        }

        public void PrebakeSimulation(CommandBuffer cmd, WaterSystem waterInstance, Vector3 waterPosition, int areaSize, int resolution, float flowSpeed, float foamStrength)
        {
            var areaPosition = waterPosition;

            areaPosition += ComputeAreaSimulationJitter(1f * areaSize / resolution, 1f * areaSize / resolution);
            if (lastPosition == null) lastPosition = areaPosition;
            var offset = areaPosition - lastPosition;

          
            if (lastWidth != resolution || lastHeight != resolution) InitializeTextures(resolution, resolution);
            SetShaderParams(cmd, waterInstance);

            cmd.SetGlobalFloat(KW_FluidsVelocityAreaScale, (0.5f * areaSize / 40f) * flowSpeed);
            cmd.SetGlobalFloat(KW_FluidsMapAreaSize_lod0,  areaSize);
            cmd.SetGlobalFloat(KW_FluidsMapAreaSize_lod1,  areaSize * 4);

            cmd.SetGlobalVector(KW_FluidsMapWorldPosition_lod0, areaPosition);
            cmd.SetGlobalVector(KW_FluidsMapWorldPosition_lod1, areaPosition * 4);

            InitializeMaterials(waterInstance);
            cmd.SetKeyword(KW_FLUIDS_PREBAKE_SIM, true);
            cmd.SetGlobalVector(KWS_ShaderConstants.ConstantWaterParams.KW_FlowMapOffset, waterInstance.Settings.FlowMapAreaPosition);
            cmd.SetGlobalFloat(KWS_ShaderConstants.ConstantWaterParams.KW_FlowMapSize,  waterInstance.Settings.FlowMapAreaSize);

            var target_lod0 = RenderFluidLod(cmd, isBakingMode: true, fluidsData[2], fluidsData[3], flowSpeed * 1.0f, areaSize, foamStrength * 5f, (Vector3)offset, areaPosition, false);

            cmd.SetGlobalTexture(KW_Fluids_Lod0, target_lod0.DataRT.rt);
            cmd.SetGlobalTexture(KW_FluidsFoam_Lod0, target_lod0.FoamRT.rt);

            prebakedFluidsRT = target_lod0.DataRT;

            lastPosition = areaPosition;
            frameNumber++;

        }

        public void SavePrebakedSimulation(WaterSystem waterInstance)
        {
#if UNITY_EDITOR
            if (prebakedFluidsRT == null) return;

            var pathToSceneFolder = KW_Extensions.GetPathToSceneInstanceFolder();
            var pathToFile        = Path.Combine(pathToSceneFolder, KWS_Settings.DataPaths.FluidsPrebakedTexture);

            InitializeMaterials(waterInstance);

            var tempRT = new RenderTexture(prebakedFluidsRT.rt.width, prebakedFluidsRT.rt.height, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
            Graphics.Blit(prebakedFluidsRT.rt, tempRT, fluidsMaterial, pass : _passRemap);
            tempRT.SaveRenderTexture(pathToFile, useAutomaticCompressionFormat: true, KW_Extensions.UsedChannels._RGBA, isHDR: false, mipChain: false);
            tempRT.Release();

            waterInstance.Settings.FlowingScriptableData.FluidsPrebakedTexture = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(pathToFile + ".kwsTexture");
            UnityEditor.EditorUtility.SetDirty(waterInstance.Settings.FlowingScriptableData);

            frameNumber = 0;
#endif
        }


        public void RenderFluids(Camera cam, CommandBuffer cmd, WaterSystem waterInstance)
        {
            var flowingData = waterInstance.Settings.FlowingScriptableData;
            if (flowingData == null || flowingData.FluidsMaskTexture == null || flowingData.FluidsPrebakedTexture == null) return;

            InitializeMaterials(waterInstance);

            var areaSize     = waterInstance.Settings.FluidsAreaSize;
            var resolution   = waterInstance.Settings.FluidsTextureSize;
            var flowSpeed    = waterInstance.Settings.FluidsSpeed;
            var foamStrength = waterInstance.Settings.FluidsFoamStrength;

            //var centerAreaPosition = RayToWaterPos(cam, waterInstance.WaterPivotWorldPosition.y);
            //var areaPosition0 = centerAreaPosition + 0*cam.transform.forward * areaSize * 0.5f;
            var pos = cam.GetCameraPositionFast();

            pos += ComputeAreaSimulationJitter(1f * areaSize / resolution, 1f * areaSize / resolution);

            // Debug.Log(areaPosition0);

            if (lastPosition == null) lastPosition = pos;
            var offset                             = pos - lastPosition;

            var lod1Multiplier = KWS_Settings.Flowing.AreaSizeMultiplierLod1;
            var areaPosition1  = pos;
            areaPosition1 += ComputeAreaSimulationJitter(lod1Multiplier * areaSize / resolution, lod1Multiplier * areaSize / resolution);
            if (lastPosition_lod1 == null) lastPosition_lod1 = areaPosition1;
            var offset_lod1                                  = areaPosition1 - lastPosition_lod1;

            if (lastWidth != resolution || lastHeight != resolution) InitializeTextures(resolution, resolution);
            SetShaderParams(cmd, waterInstance);

            cmd.SetGlobalFloat(KW_FluidsRequiredReadPrebakedSim, frameNumber < 15 ? 1 : 0); //lods * max iterrations
            cmd.SetGlobalFloat(KW_FluidsMapAreaSize_lod1,        areaSize * lod1Multiplier);
            cmd.SetGlobalVector(KW_FluidsMapWorldPosition_lod1, areaPosition1);

            cmd.SetGlobalVector(KWS_ShaderConstants.ConstantWaterParams.KW_FlowMapOffset, waterInstance.Settings.FlowMapAreaPosition);
            cmd.SetGlobalFloat(KWS_ShaderConstants.ConstantWaterParams.KW_FlowMapSize, waterInstance.Settings.FlowMapAreaSize);

            cmd.SetKeyword(KW_FLUIDS_PREBAKE_SIM, false);

            var target1 = RenderFluidLod(cmd, isBakingMode: false, fluidsData[0], fluidsData[1], flowSpeed * 0.5f, areaSize * lod1Multiplier, foamStrength * 2.5f, (Vector3)offset_lod1, areaPosition1, false);
            var target0 = RenderFluidLod(cmd, isBakingMode: false, fluidsData[2], fluidsData[3], flowSpeed * 1.0f, areaSize,                  foamStrength * 5f,   (Vector3)offset,      pos,           true, target1.DataRT.rt);

            waterInstance.InstanceData.FluidsAreaSize         = areaSize;
            waterInstance.InstanceData.FluidsSpeed            = flowSpeed;
            waterInstance.InstanceData.FluidsAreaPositionLod0 = pos;
            waterInstance.InstanceData.FluidsAreaPositionLod1 = areaPosition1;
            waterInstance.InstanceData.FluidsRT[0]            = target0.DataRT.rt;
            waterInstance.InstanceData.FluidsRT[1]            = target1.DataRT.rt;
            waterInstance.InstanceData.FluidsFoamRT[0]        = target0.FoamRT.rt;
            waterInstance.InstanceData.FluidsFoamRT[1]        = target1.FoamRT.rt;
            waterInstance.InstanceData.FluidsMask[0]          = fluidsData[2].DepthStencilMask.rt;
            waterInstance.InstanceData.FluidsMask[1]          = fluidsData[0].DepthStencilMask.rt;

            cmd.SetGlobalVector(KWS_ShaderConstants.FlowmapID.KW_FluidsMapWorldPosition_lod0, pos);
            cmd.SetGlobalVector(KWS_ShaderConstants.FlowmapID.KW_FluidsMapWorldPosition_lod1, areaPosition1);

            waterInstance.InstanceData.UpdateDynamicShaderParams(_cmd, WaterInstanceResources.DynamicPassEnum.FluidsSimulation);

            lastPosition      = pos;
            lastPosition_lod1 = areaPosition1;

            frameNumber++;

        }

        void RenderDepthPrepass(Camera cam, CommandBuffer cmd, WaterSystem waterInstance, RTHandle depthMask, float areaSize, Vector3 worldPos)
        {
            KWS_CoreUtils.SetOrthoMatrix_VP(cmd, areaSize, worldPos);
            CoreUtils.SetRenderTarget(_cmd, depthMask, depthMask, ClearFlag.All, Color.black);
            var matrix = Matrix4x4.TRS(waterInstance.WaterPivotWorldPosition, waterInstance.WaterRootTransform.rotation, waterInstance.WaterRootTransform.localScale);
            _cmd.DrawMesh(waterInstance.SplineRiverMesh, matrix, fluidsMaterial, 0, shaderPass: _passDepthMask);
        }

        FluidsData RenderFluidLod(CommandBuffer cmd,           bool          isBakingMode, FluidsData data1, FluidsData data2, float flowSpeedMultiplier, float areaSize, float foamTexelOffset, Vector3 offset, Vector3 worldPos,
                                  bool          canUseNextLod, RenderTexture nextLod = null)
        {
            cmd.SetKeyword("CAN_USE_NEXT_LOD", canUseNextLod);
            cmd.SetGlobalTexture("_FluidsNextLod", nextLod);
            cmd.SetGlobalFloat("_FlowSpeed",       flowSpeedMultiplier);
            cmd.SetGlobalFloat("_AreaSize",        areaSize);
            cmd.SetGlobalFloat("_FoamTexelOffset", foamTexelOffset);

            var source = (frameNumber % 2 == 0) ? data1 : data2;
            var target = (frameNumber % 2 == 0) ? data2 : data1;
            cmd.SetGlobalVector("_CurrentPositionOffset",   offset / areaSize);
            cmd.SetGlobalVector("_CurrentFluidMapWorldPos", worldPos);
           
            CoreUtils.SetRenderTarget(cmd, KWS_CoreUtils.GetMrt(target.DataRT, target.FoamRT), data1.DepthStencilMask, ClearFlag.None, Color.clear);
            cmd.BlitTriangle(source.DataRT, fluidsMaterial, isBakingMode ? _passBaking : _passCompute);
            return target;
        }

    }
}