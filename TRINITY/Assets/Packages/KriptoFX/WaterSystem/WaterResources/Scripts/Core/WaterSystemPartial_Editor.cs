using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif



namespace KWS
{
    public partial class WaterSystem
    {
        internal int BakedFluidsSimPercentPassed;
        internal bool _isFluidsSimBakedMode;

        internal int BakeFluidsLimitFrames = 500;
        internal int currentBakeFluidsFrames = 0;

        internal bool _isCustomMeshHasIncorrectVertexColor;

        [Flags]
        public enum WaterTab
        {
            ColorSettings       = 1,
            Waves               = 2,
            Reflection          = 4,
            ColorRefraction     = 8,
            Flow                = 16,
            DynamicWaves        = 32,
            Shoreline           = 64,
            Foam                = 128,
            VolumetricLighting  = 256,
            Caustic             = 512,
            Underwater          = 1024,
            Mesh                = 2048,
            Rendering           = 4096,
            OrthoDepth          = 8192,
            Transform           = 16384,
            TransformWaterLevel = 32768,
            All                 = ~0
        }

        internal WaterSystemScriptableData GetGlobalSettings()
        {
            var globalSettings = WaterSharedResources.GlobalSettings;
            return globalSettings != null ? globalSettings : Settings;
        }

        void CopySettings(WaterSystemScriptableData source, WaterSystemScriptableData target, WaterTab changedTab)
        {
            if (changedTab.HasTab(WaterTab.Waves))
            {
                target.GlobalWindZone                     = source.GlobalWindZone;
                target.GlobalWindZoneSpeedMultiplier      = source.GlobalWindZoneSpeedMultiplier;
                target.GlobalWindZoneTurbulenceMultiplier = source.GlobalWindZoneTurbulenceMultiplier;

                target.GlobalWindSpeed      = source.GlobalWindSpeed;
                target.GlobalWindRotation   = source.GlobalWindRotation;
                target.GlobalWindTurbulence = source.GlobalWindTurbulence;

                target.GlobalFftWavesQuality  = source.GlobalFftWavesQuality;
                target.GlobalFftWavesCascades = source.GlobalFftWavesCascades;
                target.GlobalWavesAreaScale   = source.GlobalWavesAreaScale;
                target.GlobalTimeScale        = source.GlobalTimeScale;
            }

            if (changedTab.HasTab(WaterTab.Reflection))
            {
                target.ScreenSpaceReflectionResolutionQuality = source.ScreenSpaceReflectionResolutionQuality;
                target.UseScreenSpaceReflectionHolesFilling   = source.UseScreenSpaceReflectionHolesFilling;
                target.ScreenSpaceBordersStretching           = source.ScreenSpaceBordersStretching;
                target.UseAnisotropicReflections              = source.UseAnisotropicReflections;
                target.AnisotropicReflectionsScale            = source.AnisotropicReflectionsScale;
                target.AnisotropicReflectionsHighQuality      = source.AnisotropicReflectionsHighQuality;
                target.UseScreenSpaceReflectionSky            = source.UseScreenSpaceReflectionSky;

            }

            if (changedTab.HasTab(WaterTab.Flow))
            {
                target.FluidsAreaSize = source.FluidsAreaSize;

                target.FluidsSimulationIterrations = source.FluidsSimulationIterrations;
                target.FluidsTextureSize           = source.FluidsTextureSize;
                target.FluidsSimulationFPS         = source.FluidsSimulationFPS;
                target.FluidsSpeed                 = source.FluidsSpeed;
                target.FluidsFoamStrength          = source.FluidsFoamStrength;

            }


            if (changedTab.HasTab(WaterTab.DynamicWaves))
            {
                //target.UseDynamicWaves                = source.UseDynamicWaves;
                target.DynamicWavesResolutionPerMeter = source.DynamicWavesResolutionPerMeter;
                target.DynamicWavesAreaSize           = source.DynamicWavesAreaSize;
                target.DynamicWavesPropagationSpeed   = source.DynamicWavesPropagationSpeed;
                target.DynamicWavesGlobalForceScale   = source.DynamicWavesGlobalForceScale;
                target.UseDynamicWavesRainEffect      = source.UseDynamicWavesRainEffect;
                target.DynamicWavesRainStrength       = source.DynamicWavesRainStrength;
            }


            if (changedTab.HasTab(WaterTab.VolumetricLighting))
            {
                //target.UseVolumetricLight                        = source.UseVolumetricLight;
                target.VolumetricLightResolutionQuality                      = source.VolumetricLightResolutionQuality;
                target.VolumetricLightIteration                              = source.VolumetricLightIteration;
                target.VolumetricLightTemporalReprojectionAccumulationFactor = source.VolumetricLightTemporalReprojectionAccumulationFactor;
            }


            if (changedTab.HasTab(WaterTab.Caustic))
            {
                //target.UseCausticEffect                      = source.UseCausticEffect;
                target.GlobalCausticTextureResolutionQuality = source.GlobalCausticTextureResolutionQuality;
                target.UseGlobalCausticHighQualityFiltering  = source.UseGlobalCausticHighQualityFiltering;
                target.GlobalCausticDepth                    = source.GlobalCausticDepth;
            }

            if (changedTab.HasTab(WaterTab.Underwater))
            {
                target.UseUnderwaterEffect                = source.UseUnderwaterEffect;
                target.UnderwaterReflectionMode           = source.UnderwaterReflectionMode;
                target.UseUnderwaterHalfLineTensionEffect = source.UseUnderwaterHalfLineTensionEffect;
                target.UnderwaterHalfLineTensionScale     = source.UnderwaterHalfLineTensionScale;
            }

            if (changedTab.HasTab(WaterTab.Rendering))
            {
                target.TransparentSortingPriority = source.TransparentSortingPriority;
            }
        }

        void UpdateOtherWaterInstancesGlobalSettings(WaterTab changedTab)
        {
            //instanceSettings. = Settings.;
            foreach (var waterInstance in WaterSharedResources.WaterInstances)
            {
                if (waterInstance != null) CopySettings(Settings, waterInstance.Settings, changedTab);
            }
        }

        void SetCurrentWaterInstanceSettingsFromGlobalSettings()
        {
            CopySettings(GetGlobalSettings(), Settings, WaterTab.All);
        }

        internal int GetVisibleMeshTrianglesCount()
        {
            if (!_meshQuadTree.TryGetRenderingContext(_currentCamera, false, out var context)) return 0;
          
            return context.visibleNodes.Count * _meshQuadTree.InstanceMeshes[context.activeMeshDetailingInstanceIndex].triangles.Length;
        }

        bool IsCanRendererGameOrSceneCameraOnly()
        {
            if (Application.isPlaying)
            {
                return _currentCamera.cameraType == CameraType.Game;
            }
            else
            {
                return _currentCamera.cameraType == CameraType.SceneView;
            }
        }


        internal void BakeFluidSimulation()
        {
            _isFluidsSimBakedMode = true;
            currentBakeFluidsFrames = 0;
        }


        internal int GetBakeSimulationPercent()
        {
            return _isFluidsSimBakedMode ? BakedFluidsSimPercentPassed : -1;
        }

        internal bool FluidsSimulationInEditMode()
        {
            return _isFluidsSimBakedMode;
        }

        internal void ForceUpdateFlowmapShaderParams()
        {
            var mat     = InstanceData.GetCurrentWaterMaterial();
            var flowmap = InstanceData.Flowmap != null ? InstanceData.Flowmap : Settings.FlowingScriptableData?.FlowmapTexture;

            mat.SetTexture(KWS_ShaderConstants.FlowmapID.KW_FlowMapTex, flowmap.GetSafeTexture(Color.gray));

            KWS_CoreUtils.SetVectors(mat, (KWS_ShaderConstants.ConstantWaterParams.KW_FlowMapOffset, Settings.FlowMapAreaPosition));
            KWS_CoreUtils.SetFloats(mat, (KWS_ShaderConstants.ConstantWaterParams.KW_FlowMapSize, Settings.FlowMapAreaSize),
                                    (KWS_ShaderConstants.ConstantWaterParams.KW_FlowMapFluidsStrength, Settings.FluidsFoamStrength));

        }


        internal List<ShorelineWavesScriptableData.ShorelineWave> GetShorelineWaves()
        {
            //return ShorelineWavesComponent?.GetInitializedWaves();
            if (Settings.ShorelineWavesScriptableData == null) Settings.ShorelineWavesScriptableData = ScriptableObject.CreateInstance<ShorelineWavesScriptableData>();
            return Settings.ShorelineWavesScriptableData.Waves;
        }

        internal void CheckCustomMeshCorrectVertexColor()
        {
            if (Settings.CustomMesh == null) return;

            var color = Settings.CustomMesh.colors;
            if (color.Length > 0)
            {
                _isCustomMeshHasIncorrectVertexColor = true;
                for (int i = 0; i < color.Length; i++)
                {
                    if (color[i].r > 0.0001f)
                    {
                        _isCustomMeshHasIncorrectVertexColor = false;
                        return;
                    }
                }
            }
            else
            {
                _isCustomMeshHasIncorrectVertexColor = false;
            }
         
        }

#if KWS_DEBUG


        void OnDrawGizmos()
        {
            if (DebugAABB) Gizmos.DrawWireCube(WorldSpaceBounds.center, WorldSpaceBounds.size);
            if (DebugQuadtree) DebugHelpers.DebugQuadtree(this);
            if (DebugBuoyancy && Application.isPlaying) DebugHelpers.DebugBuoyancy();
        }

        void OnGUI()
        {
            if (DebugFft) DebugHelpers.DebugFft();
            if (DebugDynamicWaves) DebugHelpers.DebugDynamicWaves();
            if (DebugOrthoDepth) DebugHelpers.DebugOrthoDepth();
           
            if (RebuildMaxHeight && Application.isPlaying)
            {
                Debug.Log("Rebuild started");
                StartCoroutine(DebugHelpers.EvaluateMaxAmplitudeTable(this));
                RebuildMaxHeight = false;
            }
        }

#endif


        void CheckCopyPastAndReplaceInstanceID()
        {
#if UNITY_EDITOR
            Event e = Event.current;
            if (e != null && (e.commandName == "Duplicate" || e.commandName == "Paste"))
            {
                _waterGUID = CreateWaterInstanceID();
                Settings = ScriptableObject.Instantiate(Settings);
            }
#endif
        }


#if UNITY_EDITOR
        [MenuItem("GameObject/Effects/KWS Water System")]
        static void CreateWaterSystemEditor(MenuCommand menuCommand)
        {
            var go = new GameObject("Water System");
            go.transform.position = SceneView.lastActiveSceneView.camera.transform.TransformPoint(Vector3.forward * 3f);

            var isOceanEnabled = WaterSharedResources.WaterInstances.Count > 0
                               && WaterSharedResources.WaterInstances.Any(w => w.Settings.WaterMeshType == WaterSystemScriptableData.WaterMeshTypeEnum.InfiniteOcean);

            var waterInstance = go.AddComponent<WaterSystem>();

            if (isOceanEnabled)
            {
                waterInstance.Settings.WaterMeshType = WaterSystemScriptableData.WaterMeshTypeEnum.FiniteBox;
                waterInstance.transform.localScale   = new Vector3(20, 10, 20);
                waterInstance.ForceUpdateWaterSettings();
            }

            go.layer = KWS_Settings.Water.WaterLayer;
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            Selection.activeObject = go;
        }
#endif

#if UNITY_EDITOR
        [MenuItem("GameObject/Effects/KWS Water Extrude Volume")]
        static void CreateExtrudeVolumeEditor(MenuCommand menuCommand)
        {
            var go = new GameObject("Water Extrude Volume");
            go.transform.position   = SceneView.lastActiveSceneView.camera.transform.TransformPoint(Vector3.forward * 3f);
            go.transform.localScale = new Vector3(10, 10, 10);

            var volumeMask = go.AddComponent<KWS_ExtrudeVolume>();
            
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            Selection.activeObject = go;
        }
#endif


    }

}