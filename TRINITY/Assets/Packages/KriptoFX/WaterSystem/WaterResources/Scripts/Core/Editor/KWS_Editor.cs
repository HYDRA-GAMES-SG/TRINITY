#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using static KWS.KWS_EditorUtils;
using Debug = UnityEngine.Debug;
using static KWS.WaterSystem;
using static KWS.KWS_Settings;
using static KWS.WaterSystemScriptableData;
using Description = KWS.KWS_EditorTextDescription;
using link = KWS.KWS_EditorUrlLinks;

namespace KWS
{
    [System.Serializable]
    [CustomEditor(typeof(WaterSystem))]
    internal partial class KWS_Editor : Editor
    {
        private WaterSystem _waterInstance;
        private WaterSystemScriptableData _settings;

        private bool _isActive;
        private WaterEditorModeEnum _waterEditorMode;
        private WaterEditorModeEnum _waterEditorModeLast;


        static KWS_EditorProfiles.PerfomanceProfiles.Reflection _reflectionProfile;
        static KWS_EditorProfiles.PerfomanceProfiles.ColorRerfraction _colorRefractionProfile;
        static KWS_EditorProfiles.PerfomanceProfiles.Flowing _flowingProfile;
        static KWS_EditorProfiles.PerfomanceProfiles.DynamicWaves _dynamicWavesProfile;
        static KWS_EditorProfiles.PerfomanceProfiles.Shoreline _shorelineProfile;
        static KWS_EditorProfiles.PerfomanceProfiles.Foam _foamProfile;
        static KWS_EditorProfiles.PerfomanceProfiles.VolumetricLight _volumetricLightProfile;
        static KWS_EditorProfiles.PerfomanceProfiles.Caustic _causticProfile;
        static KWS_EditorProfiles.PerfomanceProfiles.Mesh _meshProfile;
        static KWS_EditorProfiles.PerfomanceProfiles.Rendering _renderingProfile;


        private SceneView.SceneViewState _lastSceneView;

        private KWS_EditorSplineMesh SplineMeshEditor
        {
            get
            {
                if (_waterInstance._splineMeshEditor == null) _waterInstance._splineMeshEditor = new KWS_EditorSplineMesh(_waterInstance);
                return _waterInstance._splineMeshEditor;
            }
        }

        enum WaterEditorModeEnum
        {
            Default,
            ShorelineEditor,
            FlowmapEditor,
            FluidsEditor,
            SplineMeshEditor
        }

        string GetProfileName()
        {
            //return $"{GetNormalizedSceneName() }.{KWS_Settings.ResourcesPaths.WaterSettingsProfileAssetName}.{_waterInstance.WaterInstanceID}";
            return _waterInstance.WaterInstanceID;
        }


        void OnDestroy()
        {
            KWS_EditorUtils.Release();
        }


        public override void OnInspectorGUI()
        {
            _waterInstance = (WaterSystem)target;


            if (_waterInstance.enabled && _waterInstance.gameObject.activeSelf)
            {
                _isActive = true;
                GUI.enabled = true;
            }
            else
            {
                _isActive = false;
                GUI.enabled = false;
            }

            UpdateWaterGUI();

        }


        void OnEnable()
        {
            SceneView.duringSceneGui += OnSceneGUICustom;
        }


        void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUICustom;
        }

        void OnSceneGUICustom(SceneView sceneView)
        {
            DrawWaterEditor();
            if (Event.current.type == EventType.Repaint)
            {
                SceneView.RepaintAll();
            }
        }


        void DrawWaterEditor()
        {
            if (!_isActive) return;

            if (_waterInstance.ShorelineInEditMode) _waterEditorMode = WaterEditorModeEnum.ShorelineEditor;
            else if (_waterInstance.FlowMapInEditMode) _waterEditorMode = WaterEditorModeEnum.FlowmapEditor;
            else if (_waterInstance.FluidsSimulationInEditMode()) _waterEditorMode = WaterEditorModeEnum.FluidsEditor;
            else if (_waterInstance.SplineMeshInEditMode) _waterEditorMode = WaterEditorModeEnum.SplineMeshEditor;
            else _waterEditorMode = WaterEditorModeEnum.Default;

            switch (_waterEditorMode)
            {
                case WaterEditorModeEnum.Default:
                    break;
                case WaterEditorModeEnum.ShorelineEditor:
                    _waterInstance._shorelineEditor.DrawShorelineEditor(_waterInstance);
                    _waterInstance.ShowShorelineMap = true;
                    break;
                case WaterEditorModeEnum.FlowmapEditor:
                    _waterInstance._flowmapEditor.DrawFlowMapEditor(_waterInstance, this);
                    _waterInstance.ShowFlowMap = true;
                    break;
                case WaterEditorModeEnum.SplineMeshEditor:
                    SplineMeshEditor.DrawSplineMeshEditor(target);
                    _waterInstance.ShowMeshSettings = true;
                    break;
            }

            if (_waterEditorMode != WaterEditorModeEnum.Default || _waterEditorModeLast != _waterEditorMode) Repaint();
            _waterEditorModeLast = _waterEditorMode;
        }

        void UpdateWaterGUI()
        {
            _settings = _waterInstance.Settings;
            if (_settings == null)
            {
                Debug.Log("empty settings?");
                return;
            }

            Undo.RecordObject(_waterInstance.Settings, "Changed water parameters");
#if KWS_DEBUG
            WaterSystem.Test4 = EditorGUILayout.Vector4Field("Test4", WaterSystem.Test4);
            if (KWS_CoreUtils.SinglePassStereoEnabled) VRScale = Slider("VR Scale", "", VRScale, 0.5f, 2.5f, "");
#endif
            //waterSystem.TestObj = (GameObject) EditorGUILayout.ObjectField(waterSystem.TestObj, typeof(GameObject), true);


            EditorGUI.BeginChangeCheck();


            CheckMessages();

            var isActiveTab = _waterEditorMode == WaterEditorModeEnum.Default && _isActive;
            GUI.enabled = isActiveTab;

            bool defaultVal = false;

            _waterInstance.Profile = (WaterSystemScriptableData)EditorGUILayout.ObjectField("Settings Profile", _waterInstance.Profile, typeof(WaterSystemScriptableData), true);

            var isProfileNonSynchronized = !_waterInstance.Settings.CompareValues(_waterInstance.Profile);
            EditorGUILayout.LabelField("Settings are not synchronized with the profile", isProfileNonSynchronized ? NotesLabelStyleInfo : NotesLabelStyleEmpty);

            GUILayout.BeginHorizontal();
            
            if (SaveButton("Save to Profile", isProfileNonSynchronized, UnityEditor.EditorStyles.miniButtonLeft))
            {
                _waterInstance.Profile = _waterInstance.Settings.SaveScriptableData(_waterInstance.WaterInstanceID, _waterInstance.Profile, "WaterProfile");
                GUIUtility.ExitGUI(); //not sure what its doing, but it require to avoid  "EndLayoutGroup: BeginLayoutGroup must be called first" error
            }

            if (SaveButton("Load from Profile", false, UnityEditor.EditorStyles.miniButtonRight))
            {
                if (_waterInstance.Profile != null)
                {
                    if (EditorUtility.DisplayDialog("Load profile", "Are you sure you want to overwrite the current water settings?", "Yes", "Cancel"))
                    {
                        _waterInstance.Settings = ScriptableObject.Instantiate(_waterInstance.Profile);
                        EditorUtility.SetDirty(_waterInstance.Settings);
                        _settings = _waterInstance.Settings;
                        Debug.Log("Water settings loaded from profile file " + GetProfileName());
                        _waterInstance.ForceUpdateWaterSettings();
                    }
                }
                else
                {
                    KWS_EditorUtils.DisplayMessageNotification("Unable to load settings, profile not selected", false);
                }

            }
            
            GUILayout.EndHorizontal();


            EditorGUILayout.Space(20);

            KWS_Tab(_waterInstance, ref _waterInstance.ShowColorSettings, useHelpBox: false, useExpertButton: false, ref defaultVal, null, "Color Settings", ColorSettings, WaterTab.ColorSettings);
            KWS_Tab(_waterInstance, ref _waterInstance.ShowWaves, useHelpBox: false, useExpertButton: false, ref defaultVal, profileInterface: null, "Waves", WavesSettings, WaterTab.Waves);
            KWS_Tab(_waterInstance, ref _waterInstance.ShowFoam, useHelpBox: false, useExpertButton: false, ref defaultVal, profileInterface: null, "Foam(beta)", FoamSetting, WaterTab.Foam);
            KWS_Tab(_waterInstance, ref _waterInstance.ShowReflectionSettings, true, true, ref _waterInstance.ShowExpertReflectionSettings, _reflectionProfile, "Reflection", ReflectionSettings, WaterTab.Reflection);
            KWS_Tab(_waterInstance, ref _waterInstance.ShowRefractionSettings, true, false, ref defaultVal, _colorRefractionProfile, "Color Refraction", RefractionSetting, WaterTab.ColorRefraction);

            KWS_Tab(_waterInstance, isActiveTab, ref _settings.UseFlowMap, ref _waterInstance.ShowFlowMap, true, ref _waterInstance.ShowExpertFlowmapSettings, _flowingProfile, "Flow", FlowSettings, WaterTab.Flow);
            KWS_Tab(_waterInstance, isActiveTab, ref _settings.UseDynamicWaves, ref _waterInstance.ShowDynamicWaves, false, ref defaultVal, _dynamicWavesProfile, "Dynamic Waves", DynamicWavesSettings, WaterTab.DynamicWaves);
            KWS_Tab(_waterInstance, isActiveTab, ref _settings.UseShorelineRendering, ref _waterInstance.ShowShorelineMap, false, ref defaultVal, _shorelineProfile, "Shoreline", ShorelineSetting, WaterTab.Shoreline);
            KWS_Tab(_waterInstance, isActiveTab, ref _settings.UseVolumetricLight, ref _waterInstance.ShowVolumetricLightSettings, false, ref defaultVal, _volumetricLightProfile, "Volumetric Lighting", VolumetricLightingSettings, WaterTab.VolumetricLighting);
            KWS_Tab(_waterInstance, isActiveTab, ref _settings.UseCausticEffect, ref _waterInstance.ShowCausticEffectSettings, useExpertButton: false, ref defaultVal, _causticProfile, "Caustic", CausticSettings, WaterTab.Caustic);
            KWS_Tab(_waterInstance, isActiveTab, ref _settings.UseUnderwaterEffect, ref _waterInstance.ShowUnderwaterEffectSettings, false, ref defaultVal, null, "Underwater", UnderwaterSettings, WaterTab.Underwater);

            KWS_Tab(_waterInstance, ref _waterInstance.ShowMeshSettings, useHelpBox: false, useExpertButton: false, ref defaultVal, _meshProfile, "Mesh", MeshSettings, WaterTab.Mesh);
            KWS_Tab(_waterInstance, ref _waterInstance.ShowRendering, useHelpBox: false, useExpertButton: false, ref defaultVal, _renderingProfile, "Rendering", RenderingSetting, WaterTab.Rendering);


            GUI.enabled = isActiveTab;

            if (!_settings.UseFlowMap || !_isActive) _waterInstance.FlowMapInEditMode = false;
            if (!_settings.UseShorelineRendering || !_isActive) _waterInstance.ShorelineInEditMode = false;

            EditorGUILayout.LabelField("Water unique ID: " + _waterInstance.WaterInstanceID, KWS_EditorUtils.NotesLabelStyleFade);

            if (EditorGUI.EndChangeCheck())
            {
                if (!Application.isPlaying)
                {
                    EditorUtility.SetDirty(_waterInstance.Settings);
                    EditorSceneManager.MarkSceneDirty(_waterInstance.gameObject.scene);
                }
            }

        }


        //void CheckMessage_OceanDetailingFarDistance()
        //{
        //    if (_settings.WaterMeshType == WaterMeshTypeEnum.InfiniteOcean && _settings.UseUnderwaterEffect)
        //    {
        //        var cameras = Camera.allCameras;
        //        foreach (var cam in cameras)
        //        {
        //            if (KWS_CoreUtils.CanRenderCurrentCamera(cam) && cam.cameraType != CameraType.SceneView && cam.farClipPlane < _settings.OceanDetailingFarDistance)
        //            {
        //                KWS_EditorMessage($"Ocean Far Distance is greater than the '{cam.name}' Far distance, which can cause issues with underwater rendering. " + Environment.NewLine +
        //                                  $"Please reduce Mesh->OceanDetailingFarDistance or increase the camera far distance.", MessageType.Warning);
        //            }
        //        }

        //    }

        //}

        void CheckMessages()
        {
            if (_waterInstance.WaterRenderingActive == false) KWS_EditorMessage(Description.Warnings.RenderingActiveOverridden, MessageType.Warning);

            CheckPlatformSpecificMessages();

            //if (_settings.UseFlowMap && !_waterSystem.IsFlowmapInitialized()) KWS_EditorMessage(Description.Flow.FlowNotInitialized, MessageType.Warning);

            if (_settings.WaterMeshType == WaterMeshTypeEnum.FiniteBox && _waterInstance.WaterSize.y < _waterInstance.CurrentMaxWaveHeight * 2)
            {
                EditorGUILayout.HelpBox(Description.Warnings.SmallVolumeSize, MessageType.Warning);
            }

            //CheckMessage_OceanDetailingFarDistance();


            if (WaterSystem.SelectedThirdPartyFogMethod > 0) KWS_EditorMessage(Description.Rendering.ThirdPartyFogWarnign, MessageType.Info);
        }

        private bool CanUsePlanarReflection(out WaterSystem activePlanarInstance)
        {
            var canUsePlanar = true;
            if (WaterSharedResources.IsAnyWaterUsePlanar)
            {
                foreach (var water in WaterSharedResources.WaterInstances)
                {
                    if (water.Settings.UsePlanarReflection && _waterInstance != water)
                    {
                        canUsePlanar = false;
                        activePlanarInstance = water;
                        break;
                    }
                }
            }

            activePlanarInstance = _waterInstance;
            return canUsePlanar;
        }


        void ColorSettings()
        {
            _settings.Transparent    = Slider("Transparent (Meters)", Description.Color.Transparent, _settings.Transparent, 1f, 100f, link.Transparent);
            _settings.DyeColor       = ColorFieldHUE("Dye Color", Description.Color.WaterColor, _settings.DyeColor, false, false, false, link.DyeColor);
            _settings.TurbidityColor = ColorField("Turbidity Color", Description.Color.TurbidityColor, _settings.TurbidityColor, false, false, false, link.TurbidityColor);
        }

        void WavesSettings()
        {
            if (_settings.WaterMeshType == WaterMeshTypeEnum.InfiniteOcean)
            {
                EditorGUILayout.LabelField($"Ocean always uses global wind", KWS_EditorUtils.NotesLabelStyleFade);
                GUI.enabled             = false;
                _settings.UseGlobalWind = true;
                _settings.UseGlobalWind = Toggle("Use Global Wind", "", _settings.UseGlobalWind, link.UseGlobalWind);
                GUI.enabled             = _isActive;
            }
            else
            {
                _settings.UseGlobalWind = Toggle("Use Global Wind", "", _settings.UseGlobalWind, link.UseGlobalWind);
            }


            if (_settings.UseGlobalWind)
            {
                _settings.GlobalWindZone = (WindZone)EditorGUILayout.ObjectField(_settings.GlobalWindZone, typeof(WindZone), true);
                if (_settings.GlobalWindZone != null)
                {
                    _settings.GlobalWindZoneSpeedMultiplier      = Slider("Wind Speed Multiplier",      "", _settings.GlobalWindZoneSpeedMultiplier,      0.01f, 10,    link.WindZoneSpeedMultiplier);
                    _settings.GlobalWindZoneTurbulenceMultiplier = Slider("Wind Turbulence Multiplier", "", _settings.GlobalWindZoneTurbulenceMultiplier, 0.01f, 10.0f, link.WindZoneTurbulenceMultiplier);
                }
                else
                {
                    _settings.GlobalWindSpeed      = Slider("Global Wind Speed",      Description.Waves.WindSpeed,      _settings.GlobalWindSpeed,      0.1f, FFT.MaxWindSpeed, link.WindSpeed);
                    _settings.GlobalWindRotation   = Slider("Global Wind Rotation",   Description.Waves.WindRotation,   _settings.GlobalWindRotation,   0.0f, 360.0f,           link.WindRotation);
                    _settings.GlobalWindTurbulence = Slider("Global Wind Turbulence", Description.Waves.WindTurbulence, _settings.GlobalWindTurbulence, 0.0f, 1.0f,             link.WindTurbulence);
                }
                Line();
                _settings.GlobalFftWavesQuality  = (FftWavesQualityEnum)EnumPopup("Global Waves Quality", Description.Waves.FftWavesQuality, _settings.GlobalFftWavesQuality, link.FftWavesQuality);
                _settings.GlobalFftWavesCascades = IntSlider("Global Simulation Cascades", "", _settings.GlobalFftWavesCascades, 1, FFT.MaxLods, link.FftWavesCascades);
                _settings.GlobalWavesAreaScale   = Slider("Global Area Scale", "",                          _settings.GlobalWavesAreaScale, 0.2f, KWS_Settings.FFT.MaxWavesAreaScale, link.WavesAreaScale);
                _settings.GlobalTimeScale        = Slider("Global Time Scale", Description.Waves.TimeScale, _settings.GlobalTimeScale,      0.0f, 2.0f,                               link.TimeScale);
            }
            else
            {
                _settings.LocalWindSpeed      = Slider("Local Wind Speed",      Description.Waves.WindSpeed,      _settings.LocalWindSpeed,      0.1f, FFT.MaxWindSpeed, link.WindSpeed);
                _settings.LocalWindRotation   = Slider("Local Wind Rotation",   Description.Waves.WindRotation,   _settings.LocalWindRotation,   0.0f, 360.0f,           link.WindRotation);
                _settings.LocalWindTurbulence = Slider("Local Wind Turbulence", Description.Waves.WindTurbulence, _settings.LocalWindTurbulence, 0.0f, 1.0f,             link.WindTurbulence);

                Line();
                _settings.LocalFftWavesQuality  = (FftWavesQualityEnum)EnumPopup("Local Waves Quality", Description.Waves.FftWavesQuality, _settings.LocalFftWavesQuality, link.FftWavesQuality);
                _settings.LocalFftWavesCascades = IntSlider("Local Simulation Cascades", "", _settings.LocalFftWavesCascades, 1, FFT.MaxLods, "FftWavesCascades");
                _settings.LocalWavesAreaScale   = Slider("Local Area Scale", "",                          _settings.LocalWavesAreaScale, 0.2f, KWS_Settings.FFT.MaxWavesAreaScale, link.WavesAreaScale);
                _settings.LocalTimeScale        = Slider("Local Time Scale", Description.Waves.TimeScale, _settings.LocalTimeScale,      0.0f, 2.0f,                               link.TimeScale);
            }

        }


        void ReflectionSettings()
        {
            //KWS_EditorProfiles.PerfomanceProfiles.Reflection.ReadDataFromProfile(_waterSystem);

            _settings.UseScreenSpaceReflection = Toggle("Use Screen Space Reflection", Description.Reflection.UseScreenSpaceReflection, _settings.UseScreenSpaceReflection, link.UseScreenSpaceReflection);

            if (_settings.UseScreenSpaceReflection)
            {
                _settings.ScreenSpaceReflectionResolutionQuality = (ScreenSpaceReflectionResolutionQualityEnum)EnumPopup("Screen Space Resolution Quality",
                     Description.Reflection.ScreenSpaceReflectionResolutionQuality, _settings.ScreenSpaceReflectionResolutionQuality, link.ScreenSpaceReflectionResolutionQuality);

                if (_waterInstance.ShowExpertReflectionSettings)
                {
                    //_settings.UseScreenSpaceReflectionHolesFilling = Toggle("Holes Filling", "", _settings.UseScreenSpaceReflectionHolesFilling, link.UseScreenSpaceReflectionHolesFilling));
                    //_settings.UseScreenSpaceReflectionSky = Toggle("Screen Space Skybox", "", _settings.UseScreenSpaceReflectionSky, "UseScreenSpaceReflectionSky");
                    _settings.ScreenSpaceBordersStretching = Slider("Borders Stretching", "", _settings.ScreenSpaceBordersStretching, 0f, 0.05f, link.ScreenSpaceBordersStretching);
                }

                Line();
            }

            var layerNames = new List<string>();
            for (int i = 0; i <= 31; i++)
            {
                layerNames.Add(LayerMask.LayerToName(i));
            }


            var canUsePlanar = CanUsePlanarReflection(out var activePlanarInstance);

            GUI.enabled = _isActive && canUsePlanar;
            if (canUsePlanar == false)
            {
                EditorGUILayout.LabelField($"For performance reasons, planar reflections limited only for 1 instance!" +
                                           $" Planar reflections are already enabled for {activePlanarInstance.name}", KWS_EditorUtils.NotesLabelStyle);

                _settings.UsePlanarReflection = false;
            }

            _settings.UsePlanarReflection = Toggle("Use Planar Reflection", Description.Reflection.UsePlanarReflection, _settings.UsePlanarReflection, link.UsePlanarReflection);
            if (_settings.UsePlanarReflection)
            {
                EditorGUILayout.HelpBox(Description.Warnings.PlanarReflectionUsed, MessageType.Warning);
                _settings.RenderPlanarShadows = Toggle("Planar Shadows", "", _settings.RenderPlanarShadows, link.RenderPlanarShadows);

                if (Reflection.IsVolumetricsAndFogAvailable)
                    _settings.RenderPlanarVolumetricsAndFog = Toggle("Planar Volumetrics and Fog", "", _settings.RenderPlanarVolumetricsAndFog, link.RenderPlanarVolumetricsAndFog);
                if (Reflection.IsCloudRenderingAvailable) _settings.RenderPlanarClouds = Toggle("Planar Clouds", "", _settings.RenderPlanarClouds, link.RenderPlanarClouds);

                _settings.PlanarReflectionResolutionQuality =
                    (PlanarReflectionResolutionQualityEnum)EnumPopup("Planar Resolution Quality", Description.Reflection.PlanarReflectionResolutionQuality, _settings.PlanarReflectionResolutionQuality,
                                                                     link.PlanarReflectionResolutionQuality);

                var planarCullingMask = MaskField("Planar Layers Mask", Description.Reflection.PlanarCullingMask, _settings.PlanarCullingMask, layerNames.ToArray(), link.PlanarCullingMask);
                _settings.PlanarCullingMask = planarCullingMask & ~(1 << Water.WaterLayer);

            }
            GUI.enabled = _isActive;

            if (_waterInstance.ShowExpertReflectionSettings && (_settings.UsePlanarReflection || _settings.UseScreenSpaceReflection))
            {
                _settings.ReflectionClipPlaneOffset = Slider("Clip Plane Offset", Description.Reflection.ReflectionClipPlaneOffset, _settings.ReflectionClipPlaneOffset, 0, 0.07f,
                                                             link.ReflectionClipPlaneOffset);
            }

            if (Reflection.IsReflectionProbeAvailable)
            {
                _settings.UseReflectionProbes = Toggle("Use Reflection Probes", Description.Reflection.UseReflectionProbes, _settings.UseReflectionProbes, link.UseReflectionProbes);
            }

            if (_settings.UseScreenSpaceReflection || _settings.UsePlanarReflection)
            {
                Line();
                _settings.UseAnisotropicReflections = Toggle("Use Anisotropic Reflections", Description.Reflection.UseAnisotropicReflections, _settings.UseAnisotropicReflections, link.UseAnisotropicReflections);

                if (_settings.UseAnisotropicReflections && _waterInstance.ShowExpertReflectionSettings)
                {
                    _settings.AnisotropicReflectionsScale = Slider("Anisotropic Reflections Scale", Description.Reflection.AnisotropicReflectionsScale, _settings.AnisotropicReflectionsScale, 0.1f, 1.0f,
                                                                   link.AnisotropicReflectionsScale);
                    _settings.AnisotropicReflectionsHighQuality = Toggle("High Quality Anisotropic", Description.Reflection.AnisotropicReflectionsHighQuality, _settings.AnisotropicReflectionsHighQuality,
                                                                         link.AnisotropicReflectionsHighQuality);
                }

            }


            Line();

            _settings.OverrideSkyColor = Toggle("Override Sky Color", "", _settings.OverrideSkyColor, link.OverrideSkyColor);
            if (_settings.OverrideSkyColor)
            {
                _settings.CustomSkyColor = ColorField("Custom Sky Color", "", _settings.CustomSkyColor, false, false, false, link.OverrideSkyColor);
            }

            _settings.ReflectSun = Toggle("Reflect Sunlight", Description.Reflection.ReflectSun, _settings.ReflectSun, link.ReflectSun);
            if (_settings.ReflectSun)
            {
                _settings.ReflectedSunCloudinessStrength = Slider("Sun Cloudiness", Description.Reflection.ReflectedSunCloudinessStrength, _settings.ReflectedSunCloudinessStrength, 0.03f, 0.25f,
                                                                  link.ReflectedSunCloudinessStrength);
                if (_waterInstance.ShowExpertReflectionSettings)
                    _settings.ReflectedSunStrength = Slider("Sun Strength", Description.Reflection.ReflectedSunStrength, _settings.ReflectedSunStrength, 0f, 1f, link.ReflectedSunStrength);
            }

            CheckPlatformSpecificMessages_Reflection();

            //KWS_EditorProfiles.PerfomanceProfiles.Reflection.CheckDataChangesAnsSetCustomProfile(_settings);
        }

        void RefractionSetting()
        {
            if (Refraction.IsRefractionDownsampleAvailable) _settings.RefractionResolution = (RefractionResolutionEnum)EnumPopup("Resolution", "", _settings.RefractionResolution, link.RefractionResolution);
            _settings.RefractionMode = (RefractionModeEnum)EnumPopup("Refraction Mode", Description.Refraction.RefractionMode, _settings.RefractionMode, link.RefractionMode);

            if (_settings.RefractionMode == RefractionModeEnum.PhysicalAproximationIOR)
            {
                _settings.RefractionAproximatedDepth = Slider("Aproximated Depth", Description.Refraction.RefractionAproximatedDepth, _settings.RefractionAproximatedDepth, 0.25f, 10f, link.RefractionAproximatedDepth);
            }

            if (_settings.RefractionMode == RefractionModeEnum.Simple)
            {
                _settings.RefractionSimpleStrength = Slider("Strength", Description.Refraction.RefractionSimpleStrength, _settings.RefractionSimpleStrength, 0.02f, 1, link.RefractionSimpleStrength);
            }

            _settings.UseRefractionDispersion = Toggle("Use Dispersion", Description.Refraction.UseRefractionDispersion, _settings.UseRefractionDispersion, link.UseRefractionDispersion);
            if (_settings.UseRefractionDispersion)
            {
                _settings.RefractionDispersionStrength = Slider("Dispersion Strength", Description.Refraction.RefractionDispersionStrength, _settings.RefractionDispersionStrength, 0.25f, 1,
                                                                link.RefractionDispersionStrength);
            }

        }


        void FlowSettings()
        {
            EditorGUILayout.HelpBox(Description.Flow.FlowDescription, MessageType.Info);

            KWS_EditorTab(_waterEditorMode == WaterEditorModeEnum.FlowmapEditor, FlowmapEditModeSettings);

            EditorGUILayout.Space();
            _settings.UseFluidsSimulation = Toggle("Use Fluids Simulation", Description.Flow.UseFluidsSimulation, _settings.UseFluidsSimulation, link.UseFluidsSimulation);
            if (_settings.UseFluidsSimulation)
            {
                EditorGUILayout.HelpBox(Description.Flow.FluidSimulationUsage, MessageType.Info);

                var simPercent = _waterInstance.GetBakeSimulationPercent();
                var fluidsInfo = simPercent > 0 ? string.Concat(" (", simPercent, "%)") : string.Empty;
                if (GUILayout.Button("Bake Fluids Obstacles" + fluidsInfo))
                {
                    if (_settings.FlowingScriptableData == null || _settings.FlowingScriptableData.FlowmapTexture == null)
                    {
                        KWS_EditorUtils.DisplayMessageNotification("You haven't drawn a flow map yet. Use 'FlowMap Painter' and save the result.", false, 5);
                    }
                    else if (EditorUtility.DisplayDialog("Warning", "Baking may take about a minute (depending on the settings and power of your PC).", "Ready to wait", "Cancel"))
                    {
                        _waterInstance.FlowMapInEditMode = false;
                        _waterInstance.BakeFluidSimulation();
                    }
                }

                if (simPercent > 0) DisplayMessageNotification("Fluids baking: " + fluidsInfo, false, 3);

                EditorGUILayout.Space();

                if (_waterInstance.ShowExpertFlowmapSettings)
                {
                    float currentRenderedPixels = _settings.FluidsSimulationIterrations * _settings.FluidsTextureSize * _settings.FluidsTextureSize * 2f; //iterations * width * height * lodLevels
                    currentRenderedPixels = (currentRenderedPixels / 1000000f);
                    EditorGUILayout.LabelField("Current rendered pixels(less is better): " + currentRenderedPixels.ToString("0.0") + " millions", KWS_EditorUtils.NotesLabelStyleFade);
                    _settings.FluidsSimulationIterrations = IntSlider("Simulation iterations", Description.Flow.FluidsSimulationIterrations, _settings.FluidsSimulationIterrations, 1, 3,
                                                                         link.FluidsSimulationIterrations);
                    _settings.FluidsTextureSize = IntSlider("Fluids Texture Resolution", Description.Flow.FluidsTextureSize, _settings.FluidsTextureSize, 368, 2048, link.FluidsTextureSize);
                }

                _settings.FluidsAreaSize = IntSlider("Fluids Area Size", Description.Flow.FluidsAreaSize, _settings.FluidsAreaSize, 10, 80, link.FluidsAreaSize);
                _settings.FluidsSpeed = Slider("Fluids Flow Speed", Description.Flow.FluidsSpeed, _settings.FluidsSpeed, 0.25f, 1.0f, link.FluidsSpeed);
                _settings.FluidsFoamStrength = Slider("Fluids Foam Strength", Description.Flow.FluidsFoamStrength, _settings.FluidsFoamStrength, 0.0f, 1.0f, link.FluidsFoamStrength);
            }
        }

        void FlowmapEditModeSettings()
        {
            var isFlowEditMode = GUILayout.Toggle(_waterInstance.FlowMapInEditMode, "Flowmap Painter", "Button");
            if (_waterInstance.FlowMapInEditMode != isFlowEditMode)
            {
                if (isFlowEditMode)
                {
                    SetEditorCameraPosition(new Vector3(_settings.FlowMapAreaPosition.x, _waterInstance.WaterPivotWorldPosition.y + 10, _settings.FlowMapAreaPosition.z));
                    _waterInstance._flowmapEditor.InitializeFlowMapEditor(_waterInstance);
                }
                _waterInstance.FlowMapInEditMode = isFlowEditMode;
            }


            if (_waterInstance.FlowMapInEditMode)
            {
                EditorGUILayout.HelpBox(Description.Flow.FlowEditorUsage, MessageType.Info);


                _settings.FlowMapAreaPosition = Vector3Field("FlowMap Area Position", Description.Flow.FlowMapAreaPosition, _settings.FlowMapAreaPosition, link.FlowMapAreaPosition);
                _settings.FlowMapAreaPosition.y = _waterInstance.transform.position.y;

                EditorGUI.BeginChangeCheck();
                _settings.FlowMapAreaSize = IntSlider("Flowmap Area Size", Description.Flow.FlowMapAreaSize, _settings.FlowMapAreaSize, 10, 16000, link.FlowMapAreaSize);
                if (EditorGUI.EndChangeCheck()) _waterInstance._flowmapEditor.RedrawFlowMap(_waterInstance, _settings.FlowMapAreaSize);


                EditorGUI.BeginChangeCheck();
                _settings.FlowMapTextureResolution = (FlowmapTextureResolutionEnum)EnumPopup("Flowmap resolution", Description.Flow.FlowMapTextureResolution, _settings.FlowMapTextureResolution, link.FlowMapTextureResolution);
                if (EditorGUI.EndChangeCheck()) _waterInstance._flowmapEditor.ChangeFlowmapResolution(_waterInstance, (int)_settings.FlowMapTextureResolution);


                EditorGUILayout.Space();
                _settings.FlowMapSpeed = Slider("Flow Speed", Description.Flow.FlowMapSpeed, _settings.FlowMapSpeed, 0.1f, 3f, link.FlowMapSpeed);


                _waterInstance.FlowMapBrushStrength = Slider("Brush Strength", Description.Flow.FlowMapBrushStrength, _waterInstance.FlowMapBrushStrength, 0.01f, 1, nameof(_waterInstance.FlowMapBrushStrength));
                EditorGUILayout.Space();

                if (GUILayout.Button("Load Latest Saved"))
                {
                    if (EditorUtility.DisplayDialog("Load Latest Saved?", Description.Flow.LoadLatestSaved, "Yes", "Cancel"))
                    {
                        _waterInstance._flowmapEditor.LoadFlowMap(_waterInstance);
                        Debug.Log("Load Latest Saved");
                    }
                }

                if (GUILayout.Button("Delete All"))
                {
                    if (EditorUtility.DisplayDialog("Delete All?", Description.Flow.DeleteAll, "Yes", "Cancel"))
                    {
                        _waterInstance._flowmapEditor.ClearFlowMap(_waterInstance);
                        Debug.Log("Flowmap data has been deleted");
                    }
                }

                if (GUILayout.Button("Save All"))
                {
                    _waterInstance._flowmapEditor.SaveFlowMap(_waterInstance);
                    UnityEditor.AssetDatabase.Refresh();
                    Debug.Log("Flowmap texture saved");
                }

                GUILayout.Space(10);
                EditorGUI.BeginChangeCheck();
                _settings.FlowingScriptableData = (FlowingScriptableData)EditorGUILayout.ObjectField("Flowing Data", _settings.FlowingScriptableData, typeof(FlowingScriptableData), true);
                if (EditorGUI.EndChangeCheck())
                    //{
                    EditorUtility.SetDirty(_settings.FlowingScriptableData);
                //_flowmapEditor.ReinitializeFlowmap();
                //}
                GUILayout.Space(10);

                _waterInstance.ForceUpdateFlowmapShaderParams();
                GUI.enabled = _isActive;
            }
        }

        void DynamicWavesSettings()
        {
            EditorGUILayout.HelpBox(Description.DynamicWaves.Usage, MessageType.Warning);
            var maxTexSize = DynamicWaves.MaxDynamicWavesTexSize;

            int currentRenderedPixels = _settings.DynamicWavesAreaSize * _settings.DynamicWavesResolutionPerMeter;
            currentRenderedPixels = currentRenderedPixels * currentRenderedPixels;
            EditorGUILayout.LabelField($"Simulation rendered pixels (less is better): {KW_Extensions.SpaceBetweenThousand(currentRenderedPixels)}", KWS_EditorUtils.NotesLabelStyleFade);

            _settings.DynamicWavesAreaSize = IntSlider("Waves Area Size", Description.DynamicWaves.DynamicWavesAreaSize, _settings.DynamicWavesAreaSize, 10, 200, link.DynamicWavesAreaSize);
            _settings.DynamicWavesResolutionPerMeter = _settings.DynamicWavesAreaSize * _settings.DynamicWavesResolutionPerMeter > maxTexSize
                ? maxTexSize / _settings.DynamicWavesAreaSize
                : _settings.DynamicWavesResolutionPerMeter;


            _settings.DynamicWavesResolutionPerMeter = IntSlider("Detailing per meter", Description.DynamicWaves.DynamicWavesResolutionPerMeter, _settings.DynamicWavesResolutionPerMeter, 5, 50,
                                                                       link.DynamicWavesResolutionPerMeter);
            _settings.DynamicWavesAreaSize = _settings.DynamicWavesAreaSize * _settings.DynamicWavesResolutionPerMeter > maxTexSize
                ? maxTexSize / _settings.DynamicWavesResolutionPerMeter
                : _settings.DynamicWavesAreaSize;

            _settings.DynamicWavesPropagationSpeed = Slider("Speed", Description.DynamicWaves.DynamicWavesPropagationSpeed, _settings.DynamicWavesPropagationSpeed, 0.1f, 2, link.DynamicWavesPropagationSpeed);
            _settings.DynamicWavesGlobalForceScale = Slider("Global Force Scale", "", _settings.DynamicWavesGlobalForceScale, 0.05f, 1, link.DynamicWavesGlobalForceScale);

            EditorGUILayout.Space();
            _settings.UseDynamicWavesRainEffect = Toggle("Rain Drops", Description.DynamicWaves.UseDynamicWavesRainEffect, _settings.UseDynamicWavesRainEffect, link.UseDynamicWavesRainEffect);
            if (_settings.UseDynamicWavesRainEffect)
            {
                _settings.DynamicWavesRainStrength = Slider("Rain Strength", Description.DynamicWaves.DynamicWavesRainStrength, _settings.DynamicWavesRainStrength, 0.01f, 1, link.DynamicWavesRainStrength);
            }
        }

        void ShorelineSetting()
        {
            _settings.ShorelineFoamLodQuality = (ShorelineFoamQualityEnum)EnumPopup("Foam Lod Quality", "", _settings.ShorelineFoamLodQuality, link.ShorelineFoamLodQuality);
            _settings.ShorelineColor = ColorField("Shoreline Color", "", _settings.ShorelineColor, false, true, true, link.ShorelineColor);
            if (KWS_CoreUtils.IsAtomicsSupported())
                _settings.UseShorelineFoamFastMode = Toggle("Use Fast Mode", Description.Shoreline.FoamCastShadows, _settings.UseShorelineFoamFastMode, link.UseShorelineFoamFastMode);
            else
            {
                EditorGUILayout.LabelField($"Fast mode is not supported on this platform, it's directX11/12 only feature", KWS_EditorUtils.NotesLabelStyleFade);
                GUI.enabled = false;
                _settings.UseShorelineFoamFastMode = Toggle("Use Fast Mode", Description.Shoreline.FoamCastShadows, _settings.UseShorelineFoamFastMode, link.UseShorelineFoamFastMode);
                GUI.enabled = _isActive;
            }

            _settings.ShorelineFoamReceiveDirShadows = Toggle("Receive Shadows", string.Empty, _settings.ShorelineFoamReceiveDirShadows, link.ShorelineFoamReceiveDirShadows);

            KWS_EditorTab(_waterEditorMode == WaterEditorModeEnum.ShorelineEditor, ShorelineEditModeSettings);
        }

        void FoamSetting()
        {
            _settings.UseOceanFoam = Toggle("Use Ocean Foam", "", _settings.UseOceanFoam, "");
            if (_settings.UseOceanFoam)
            {
                if (_waterInstance.Settings.CurrentWindSpeed < 7.1f) EditorGUILayout.HelpBox("Foam appears during strong winds (from ~8 meters and above)", MessageType.Info);
                _settings.OceanFoamColor       = ColorField("Ocean Foam Color", "", _settings.OceanFoamColor, false, true, true, link.OceanFoamColor, false);
                _settings.OceanFoamStrength    = Slider("Ocean Foam Strength",     "", _settings.OceanFoamStrength,    0.05f, 1,  link.OceanFoamStrength, false);
                _settings.OceanFoamTextureSize = Slider("Ocean Foam Texture Size", "", _settings.OceanFoamTextureSize, 5,     50, link.TextureFoamSize, false);
                Line();
            }


            _settings.UseIntersectionFoam = Toggle("Use Intersection Foam", "", _settings.UseIntersectionFoam, "");
            if (_settings.UseIntersectionFoam)
            {
                _settings.IntersectionFoamColor        = ColorField("Foam Color", "", _settings.IntersectionFoamColor, false, true, true, link.IntersectionFoamColor, false);
                _settings.IntersectionFoamFadeDistance = Slider("Fade Distance",     "", _settings.IntersectionFoamFadeDistance, 0.01f, 10, link.IntersectionFoamFadeDistance, false);
                _settings.IntersectionTextureFoamSize  = Slider("Foam Texture Size", "", _settings.IntersectionTextureFoamSize,  5,     50, link.TextureFoamSize, false);
            }
        }

        void ShorelineEditModeSettings()
        {
            _waterInstance.ShorelineInEditMode = GUILayout.Toggle(_waterInstance.ShorelineInEditMode, "Edit Mode", "Button");

            if (_waterInstance.ShorelineInEditMode)
            {
                if (_waterInstance.UndoProvider != null && _waterInstance.UndoProvider.ShorelineWaves != null)
                {
                    _waterInstance.UndoProvider.ShorelineWaves = _waterInstance.GetShorelineWaves();
                    Undo.RecordObject(_waterInstance.UndoProvider, "Changed shoreline");
                }

                GUILayout.Space(10);
                EditorGUILayout.HelpBox(Description.Shoreline.ShorelineEditorUsage, MessageType.Info);
                GUILayout.Space(10);
                if (GUILayout.Button(new GUIContent("Add Wave")))
                {
                    _waterInstance._shorelineEditor.AddWave(_waterInstance, _waterInstance._shorelineEditor.GetCameraToWorldRay(), true);
                }

                if (GUILayout.Button("Delete All Waves"))
                {
                    if (EditorUtility.DisplayDialog("Delete Shoreline Waves?", Description.Shoreline.DeleteAll, "Yes", "Cancel"))
                    {
                        Debug.Log("Shoreline waves deleted");
                        _waterInstance._shorelineEditor.RemoveAllWaves(_waterInstance);
                    }
                }

                if (GUILayout.Button("Save Changes"))
                {
                    _waterInstance._shorelineEditor.SaveShorelineWavesData(_waterInstance);
                    Debug.Log("Shoreline Saved");
                }
                GUILayout.Space(10);

                _settings.ShorelineWavesScriptableData = (ShorelineWavesScriptableData)EditorGUILayout.ObjectField("Waves Data", _settings.ShorelineWavesScriptableData, typeof(ShorelineWavesScriptableData), true);

                GUILayout.Space(10);
            }
        }

        void VolumetricLightingSettings()
        {
            CheckPlatformSpecificMessages_VolumeLight();

            _settings.VolumetricLightResolutionQuality =
                (VolumetricLightResolutionQualityEnum)EnumPopup("Resolution Quality", Description.VolumetricLight.ResolutionQuality, _settings.VolumetricLightResolutionQuality, link.VolumetricLightResolutionQuality);
            _settings.VolumetricLightIteration = IntSlider("Iterations", Description.VolumetricLight.Iterations, _settings.VolumetricLightIteration, 2, KWS_Settings.VolumetricLighting.MaxIterations, link.VolumetricLightIteration);
            _settings.VolumetricLightTemporalReprojectionAccumulationFactor = Slider("Temporal Accumulation Factor", "", _settings.VolumetricLightTemporalReprojectionAccumulationFactor, 0.1f, 0.75f, link.VolumetricLightTemporalAccumulationFactor);

            Line();

            if (_settings.VolumetricLightUseAdditionalLightsCaustic) EditorGUILayout.HelpBox("AdditionalLightsCaustic with multiple light sources can cause dramatic performance drop.", MessageType.Warning);
            _settings.VolumetricLightUseAdditionalLightsCaustic = Toggle("Use Additional Lights Caustic", "", _settings.VolumetricLightUseAdditionalLightsCaustic, link.VolumetricLightUseAdditionalLightsCaustic);
        }

        void CausticSettings()
        {

            var size = (int)_settings.GetCurrentCausticTextureResolutionQuality;
            float currentRenderedPixels = size * size;
            currentRenderedPixels *= _settings.GetCurrentCausticHighQualityFiltering ? 2 : 1;
            currentRenderedPixels = (currentRenderedPixels / 1000000f);
            EditorGUILayout.LabelField("Simulation rendered pixels (less is better): " + currentRenderedPixels.ToString("0.0") + " millions", KWS_EditorUtils.NotesLabelStyleFade);

            if (_settings.UseGlobalWind)
            {
                _settings.GlobalCausticTextureResolutionQuality = (CausticTextureResolutionQualityEnum)EnumPopup("Global Caustic Resolution", "", _settings.GlobalCausticTextureResolutionQuality, link.CausticTextureSize);
                _settings.UseGlobalCausticHighQualityFiltering  = Toggle("Global High Quality Filtering", "", _settings.UseGlobalCausticHighQualityFiltering, link.UseCausticBicubicInterpolation);
                _settings.GlobalCausticDepth                    = Slider("Global Caustic Depth(m)", Description.Caustic.CausticDepthScale, _settings.GlobalCausticDepth, 0.5f, Caustic.MaxCausticDepth, link.CausticDepthScale);
            }
            else
            {
                _settings.LocalCausticTextureResolutionQuality = (CausticTextureResolutionQualityEnum)EnumPopup("Local Caustic Resolution", "", _settings.LocalCausticTextureResolutionQuality, link.CausticTextureSize);
                _settings.UseLocalCausticHighQualityFiltering  = Toggle("Local High Quality Filtering", "", _settings.UseLocalCausticHighQualityFiltering, link.UseCausticBicubicInterpolation);
                _settings.LocalCausticDepth                    = Slider("Local Caustic Depth(m)", Description.Caustic.CausticDepthScale, _settings.LocalCausticDepth, 0.5f, Caustic.MaxCausticDepth, link.CausticDepthScale);
            }

            Line();
            _settings.UseCausticDispersion = Toggle("Use Dispersion", Description.Caustic.UseCausticDispersion, _settings.UseCausticDispersion, link.UseCausticDispersion);
            _settings.CausticStrength = Slider("Caustic Strength", Description.Caustic.CausticStrength, _settings.CausticStrength, 0.25f, 5, link.CausticStrength);
        }


        void UnderwaterSettings()
        {
            _settings.UnderwaterReflectionMode = (UnderwaterReflectionModeEnum)EnumPopup("Internal Reflection Mode", "", _settings.UnderwaterReflectionMode, link.UnderwaterReflectionMode);

            _settings.UseUnderwaterHalfLineTensionEffect = Toggle("Use Half Line Tension Effect", "", _settings.UseUnderwaterHalfLineTensionEffect, link.UnderwaterHalfLineTensionEffect);
            if (_settings.UseUnderwaterHalfLineTensionEffect) _settings.UnderwaterHalfLineTensionScale = Slider("Tension Scale", "", _settings.UnderwaterHalfLineTensionScale, 0.2f, 1f, link.TensionScale);
          
            _settings.OverrideUnderwaterTransparent = Toggle("Override Transparent", "", _settings.OverrideUnderwaterTransparent, link.OverrideUnderwaterTransparent);
            if (_settings.OverrideUnderwaterTransparent)
            {
                _settings.UnderwaterTransparentOffset = Slider("Transparent Offset", Description.Color.Transparent, _settings.UnderwaterTransparentOffset, -100, 100, link.Transparent);
            }
            
        }

        void MeshSettings()
        {
            if (!_settings.UseTesselation)
            {
                if (_settings.CustomMesh != null)
                {
                    var vertexCount = (int)(_settings.CustomMesh.triangles.Length / 3.0f);
                    EditorGUILayout.LabelField($"Visible water triangles: {KW_Extensions.SpaceBetweenThousand(vertexCount)}", KWS_EditorUtils.NotesLabelStyleFade);
                }
                else if (_settings.WaterMeshType == WaterMeshTypeEnum.FiniteBox || _settings.WaterMeshType == WaterMeshTypeEnum.InfiniteOcean)
                {
                    var vertexCount = _waterInstance.GetVisibleMeshTrianglesCount();
                    EditorGUILayout.LabelField($"Visible water triangles in editor view: {KW_Extensions.SpaceBetweenThousand(vertexCount)}", KWS_EditorUtils.NotesLabelStyleFade);
                }

            }
            _settings.WaterMeshType = (WaterMeshTypeEnum)EnumPopup("Water Mesh Type", Description.Mesh.WaterMeshType, _settings.WaterMeshType, link.WaterMeshType);


            if (_settings.WaterMeshType == WaterMeshTypeEnum.CustomMesh)
            {
                EditorGUI.BeginChangeCheck();
                _settings.CustomMesh = (UnityEngine.Mesh)EditorGUILayout.ObjectField(_settings.CustomMesh, typeof(UnityEngine.Mesh), true);
                if (EditorGUI.EndChangeCheck())
                {
                    _waterInstance.CheckCustomMeshCorrectVertexColor();
                }

                if (_settings.CustomMesh != null && _waterInstance._isCustomMeshHasIncorrectVertexColor)
                {
                    EditorGUILayout.HelpBox("All the vertices are black, use white vertex color for correct water view", MessageType.Warning);
                }
                EditorGUILayout.HelpBox(Description.Mesh.CustomMeshUsage, MessageType.Info);
            }


            if (_settings.WaterMeshType == WaterMeshTypeEnum.River)
            {
                KWS_EditorTab(_waterEditorMode == WaterEditorModeEnum.SplineMeshEditor, SplineEditModeSettings);
            }

            if (!_settings.UseTesselation)
            {
                switch (_settings.WaterMeshType)
                {
                    case WaterMeshTypeEnum.InfiniteOcean:
                        _settings.WaterMeshQualityInfinite = (WaterMeshQualityEnum)EnumPopup("Mesh Quality", Description.Mesh.MeshQuality, _settings.WaterMeshQualityInfinite, link.WaterMeshQualityInfinite);
                        break;
                    case WaterMeshTypeEnum.FiniteBox:
                        _settings.WaterMeshQualityFinite = (WaterMeshQualityEnum)EnumPopup("Mesh Quality", Description.Mesh.MeshQuality, _settings.WaterMeshQualityFinite, link.WaterMeshQualityFinite);
                        break;
                }
            }

            if (_settings.WaterMeshType == WaterMeshTypeEnum.FiniteBox || _settings.WaterMeshType == WaterMeshTypeEnum.CustomMesh)
            {
                _settings.UseAquariumMode = Toggle("Use Aquarium Mode", "", _settings.UseAquariumMode, "UseAquariumMode");
            }


            if (_settings.WaterMeshType == WaterMeshTypeEnum.InfiniteOcean)
            {
                //CheckMessage_OceanDetailingFarDistance();
                _settings.OceanDetailingFarDistance = IntSlider("Ocean Detailing Far Distance", "", _settings.OceanDetailingFarDistance, 500, 5000, link.OceanDetailingFarDistance);
            }

            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Metal &&
                (_settings.WaterMeshType == WaterMeshTypeEnum.InfiniteOcean || _settings.WaterMeshType == WaterMeshTypeEnum.FiniteBox))
            {
                _settings.UseTesselation = false;
                EditorGUILayout.LabelField($"Tesselation on Metal API is only available for custom meshes and rivers", KWS_EditorUtils.NotesLabelStyleFade);
                GUI.enabled = false;
                _settings.UseTesselation = Toggle("Use Tesselation", Description.Mesh.UseTesselation, _settings.UseTesselation, link.UseTesselation);
                GUI.enabled = _isActive;
            }
            else
            {
                _settings.UseTesselation = Toggle("Use Tesselation", Description.Mesh.UseTesselation, _settings.UseTesselation, link.UseTesselation);
            }

            if (_settings.UseTesselation)
            {


                switch (_settings.WaterMeshType)
                {
                    case WaterMeshTypeEnum.InfiniteOcean:
                        _settings.TesselationFactor_Infinite      = Slider("Tesselation Factor",       Description.Mesh.TesselationFactor,      _settings.TesselationFactor_Infinite,      0.15f, 1,    link.TesselationFactor);
                        _settings.TesselationMaxDistance_Infinite = Slider("Tesselation Max Distance", Description.Mesh.TesselationMaxDistance, _settings.TesselationMaxDistance_Infinite, 10,    5000, link.TesselationMaxDistance);
                        break;
                    case WaterMeshTypeEnum.FiniteBox:
                        _settings.TesselationFactor_Finite      = Slider("Tesselation Factor",       Description.Mesh.TesselationFactor,      _settings.TesselationFactor_Finite,      0.15f, 1,   link.TesselationFactor);
                        _settings.TesselationMaxDistance_Finite = Slider("Tesselation Max Distance", Description.Mesh.TesselationMaxDistance, _settings.TesselationMaxDistance_Finite, 10,    300, link.TesselationMaxDistance);
                        break;
                    case WaterMeshTypeEnum.River:
                        _settings.TesselationFactor_River      = Slider("Tesselation Factor",       Description.Mesh.TesselationFactor,      _settings.TesselationFactor_River,      0.15f, 1,   link.TesselationFactor);
                        _settings.TesselationMaxDistance_River = Slider("Tesselation Max Distance", Description.Mesh.TesselationMaxDistance, _settings.TesselationMaxDistance_River, 10,    200, link.TesselationMaxDistance);
                        break;
                    case WaterMeshTypeEnum.CustomMesh:
                        _settings.TesselationFactor_Custom      = Slider("Tesselation Factor",       Description.Mesh.TesselationFactor,      _settings.TesselationFactor_Custom,      0.15f, 1,   link.TesselationFactor);
                        _settings.TesselationMaxDistance_Custom = Slider("Tesselation Max Distance", Description.Mesh.TesselationMaxDistance, _settings.TesselationMaxDistance_Custom, 10,    200, link.TesselationMaxDistance);
                        break;
                }
            }

        }

        void SplineEditModeSettings()
        {

            EditorGUI.BeginChangeCheck();
            _waterInstance.SplineMeshInEditMode = GUILayout.Toggle(_waterInstance.SplineMeshInEditMode, "River Editor", "Button");
            if (EditorGUI.EndChangeCheck())
            {
                if (_waterInstance.SplineMeshInEditMode)
                {
                    _waterInstance.SplineMeshComponent.LoadOrCreateSpline(_waterInstance);
                    _waterInstance.SplineMeshComponent.UpdateSplinePivotOffset(_waterInstance);
                }
                _settings.WireframeMode = _waterInstance.SplineMeshInEditMode;
            }

            if (_waterInstance.SplineMeshInEditMode)
            {
                EditorGUILayout.HelpBox(Description.Mesh.RiverUsage, MessageType.Info);
                GUILayout.Space(20);

                _settings.RiverSplineNormalOffset = Slider("Spline Normal Offset", Description.Mesh.RiverSplineNormalOffset, _settings.RiverSplineNormalOffset, 0.1f, 10, link.RiverSplineNormalOffset);

                EditorGUI.BeginChangeCheck();

                _settings.RiverSplineDepth = IntSlider("Selected Spline Depth", "", _settings.RiverSplineDepth, 1, 100, link.RiverSplineDepth);
                var loadedVertexCount = SplineMeshEditor.GetVertexCountBetweenPoints();
                if (loadedVertexCount == -1) loadedVertexCount = _settings.RiverSplineVertexCountBetweenPoints;
                var newVertexCountBetweenPoints = IntSlider("Selected Spline Vertex Count", Description.Mesh.RiverSplineVertexCountBetweenPoints,
                                                            loadedVertexCount, KWS_Settings.Mesh.SplineRiverMinVertexCount, KWS_Settings.Mesh.SplineRiverMaxVertexCount, link.RiverSplineVertexCountBetweenPoints);
                if (EditorGUI.EndChangeCheck())
                {
                    _settings.RiverSplineVertexCountBetweenPoints = newVertexCountBetweenPoints;
                    SplineMeshEditor.UpdateSplineParams();


                    //if (_settings.WaterMesh != null) //todo add for splineMesh
                    //{
                    //    var vertexCount = (int)(_settings.WaterMesh.triangles.Length / 3.0f);
                    //    DisplayMessageNotification($"Water mesh triangles count: {KW_Extensions.SpaceBetweenThousand(vertexCount)}", false, 1);
                    //}
                }

                if (GUILayout.Button("Add River"))
                {
                    _waterInstance.SplineMeshComponent.UpdateSplinePivotOffset(_waterInstance);
                    _waterInstance.SplineMeshComponent.SaveSplineData(_waterInstance);
                    SplineMeshEditor.AddSpline();
                }

                if (GUILayout.Button("Delete Selected River"))
                {
                    if (EditorUtility.DisplayDialog("Delete Selected River?", Description.Mesh.RiverDeleteAll, "Yes", "Cancel"))
                    {
                        SplineMeshEditor.DeleteSpline();
                        Debug.Log("Selected river deleted");
                    }
                }

                if (SaveButton("Save Changes", SplineMeshEditor.IsSplineChanged()))
                {
                    _waterInstance.SplineMeshComponent.UpdateSplinePivotOffset(_waterInstance);
                    _waterInstance.SplineMeshComponent.SaveSplineData(_waterInstance);
                    SplineMeshEditor.ResetSplineChangeStatus();
                    Debug.Log("River spline saved");
                }

            }

            EditorGUI.BeginChangeCheck();
            _settings.SplineScriptableData = (SplineScriptableData)EditorGUILayout.ObjectField("Spline Data", _settings.SplineScriptableData, typeof(SplineScriptableData), true);
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(_settings.SplineScriptableData);
                _waterInstance.SplineMeshComponent.LoadOrCreateSpline(_waterInstance);
                _waterInstance.SplineMeshComponent.UpdateAllMeshes(_waterInstance);
            }
            GUILayout.Space(10);
        }

        void RenderingSetting()
        {
            ReadSelectedThirdPartyFog();
            var selectedThirdPartyFogMethod = ThirdPartyFogAssetsDescriptions[WaterSystem.SelectedThirdPartyFogMethod];


            if (selectedThirdPartyFogMethod.CustomQueueOffset != 0)
            {
                EditorGUILayout.LabelField($"Min TransparentSortingPriority overrated by {selectedThirdPartyFogMethod.EditorName}", KWS_EditorUtils.NotesLabelStyleFade);
                _settings.TransparentSortingPriority = IntSlider("Transparent Sorting Priority", "", _settings.TransparentSortingPriority, selectedThirdPartyFogMethod.CustomQueueOffset, 50, link.TransparentSortingPriority);
            }
            else
            {
                _settings.TransparentSortingPriority = IntSlider("Transparent Sorting Priority", "", _settings.TransparentSortingPriority, -50, 50, link.TransparentSortingPriority);
            }

            //_settings.EnabledMeshRendering       = Toggle("Enabled Mesh Rendering", "", _settings.EnabledMeshRendering, link.EnabledMeshRendering), false);

            if (selectedThirdPartyFogMethod.DrawToDepth)
            {
                EditorGUILayout.LabelField($"Draw To Depth override by {selectedThirdPartyFogMethod.EditorName}", KWS_EditorUtils.NotesLabelStyleFade);
                GUI.enabled = false;
                _settings.DrawToPosteffectsDepth = Toggle("Draw To Depth", Description.Rendering.DrawToPosteffectsDepth, true, link.DrawToPosteffectsDepth);
                GUI.enabled = _isActive;
            }
            else
            {
                _settings.DrawToPosteffectsDepth = Toggle("Draw To Depth", Description.Rendering.DrawToPosteffectsDepth, _settings.DrawToPosteffectsDepth, link.DrawToPosteffectsDepth);
            }


            _waterInstance.WideAngleCameraRenderingMode = Toggle("Wide-Angle Camera Rendering Mode", "", _waterInstance.WideAngleCameraRenderingMode, link.WideAngleCameraRenderingMode);
            //if (_waterSystem.UseTesselation)
            //{
            //    _waterSystem.WireframeMode = false;
            //    EditorGUILayout.LabelField($"Wireframe mode doesn't work with tesselation (water -> mesh -> use tesselation)", KWS_EditorUtils.NotesLabelStyleFade);
            //    GUI.enabled                           = false;
            //    _waterSystem.WireframeMode = Toggle("Wireframe Mode", "", _waterSystem.WireframeMode, nameof(_waterSystem.WireframeMode));
            //    GUI.enabled = _isActive;
            //}
            //else
            //{
            //    _waterSystem.WireframeMode = Toggle("Wireframe Mode", "", _waterSystem.WireframeMode, nameof(_waterSystem.WireframeMode));
            //}

            var assets = ThirdPartyFogAssetsDescriptions;
            var fogDisplayedNames = new string[assets.Count + 1];
            for (var i = 0; i < assets.Count; i++)
            {
                fogDisplayedNames[i] = assets[i].EditorName;
            }
            EditorGUI.BeginChangeCheck();
            WaterSystem.SelectedThirdPartyFogMethod = EditorGUILayout.Popup("Third-Party Fog Support", WaterSystem.SelectedThirdPartyFogMethod, fogDisplayedNames);
            if (EditorGUI.EndChangeCheck())
            {
                UpdateThirdPartyFog();
            }

#if KWS_DEBUG
            Line();

            if (_settings.WaterMeshType == WaterMeshTypeEnum.InfiniteOcean || _settings.WaterMeshType == WaterMeshTypeEnum.FiniteBox)
            {
                WaterSystem.DebugQuadtree = Toggle("Debug Quadtree", "", WaterSystem.DebugQuadtree, "");
            }
            _waterInstance.DebugAABB         = Toggle("Debug AABB",           "", _waterInstance.DebugAABB,         "");
            _waterInstance.DebugFft          = Toggle("Debug Fft",            "", _waterInstance.DebugFft,          "");
            _waterInstance.DebugDynamicWaves = Toggle("Debug Dynamic Waves",  "", _waterInstance.DebugDynamicWaves, "");
            _waterInstance.DebugOrthoDepth   = Toggle("Debug Ortho Depth",    "", _waterInstance.DebugOrthoDepth,   "");
            _waterInstance.DebugBuoyancy     = Toggle("Debug Buoyancy",       "", _waterInstance.DebugBuoyancy,     "");
            WaterSystem.DebugUpdateManager   = Toggle("Debug Update Manager", "", WaterSystem.DebugUpdateManager, "");
            Line();
            WaterSystem.RebuildMaxHeight = Toggle("Rebuild Max Height", "", WaterSystem.RebuildMaxHeight, "");
#endif

        }

        void ReadSelectedThirdPartyFog()
        {
            //load enabled third-party asset for all water instances
            if (WaterSystem.SelectedThirdPartyFogMethod == -1)
            {
                var defines = ThirdPartyFogAssetsDescriptions.Select(n => n.ShaderDefine).ToList();
                WaterSystem.SelectedThirdPartyFogMethod = KWS_EditorUtils.GetEnabledDefineIndex(ShaderPaths.KWS_PlatformSpecificHelpers, defines);
            }

        }

        void UpdateThirdPartyFog()
        {
            if (SelectedThirdPartyFogMethod > 0)
            {
                var selectedMethod = ThirdPartyFogAssetsDescriptions[WaterSystem.SelectedThirdPartyFogMethod];
                if (!selectedMethod.IgnoreInclude)
                {
                    var inlcudeFileName = KW_Extensions.GetAssetsRelativePathToFile(selectedMethod.ShaderInclude, selectedMethod.AssetNameSearchPattern);
                    if (inlcudeFileName == String.Empty)
                    {
                        Debug.LogError($"Can't find the asset {ThirdPartyFogAssetsDescriptions[WaterSystem.SelectedThirdPartyFogMethod].EditorName}");
                        return;
                    }
                }
            }

            //replace defines
            for (int i = 1; i < ThirdPartyFogAssetsDescriptions.Count; i++)
            {
                var selectedMethod = ThirdPartyFogAssetsDescriptions[i];
                SetShaderTextDefine(ShaderPaths.KWS_PlatformSpecificHelpers, selectedMethod.ShaderDefine, WaterSystem.SelectedThirdPartyFogMethod == i);
            }

            //replace paths to assets
            if (WaterSystem.SelectedThirdPartyFogMethod > 0)
            {
                var selectedMethod = ThirdPartyFogAssetsDescriptions[WaterSystem.SelectedThirdPartyFogMethod];
                if (!selectedMethod.IgnoreInclude)
                {
                    var inlcudeFileName = KW_Extensions.GetAssetsRelativePathToFile(selectedMethod.ShaderInclude, selectedMethod.AssetNameSearchPattern);
                    KWS_EditorUtils.ChangeShaderTextIncludePath(KWS_Settings.ShaderPaths.KWS_PlatformSpecificHelpers, selectedMethod.ShaderDefine, inlcudeFileName);
                }
            }

            var thirdPartySelectedFog = ThirdPartyFogAssetsDescriptions[WaterSystem.SelectedThirdPartyFogMethod];
            if (thirdPartySelectedFog.DrawToDepth) _settings.DrawToPosteffectsDepth = true;

            AssetDatabase.Refresh();
        }
    }
}
#endif