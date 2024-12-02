#pragma warning disable 0168 // variable declared but not used.
#pragma warning disable 0219 // variable assigned but not used.
#pragma warning disable 0414 // private field assigned but not used.


using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

using static KWS.WaterSystemScriptableData;
using System.IO;
using Unity.Collections;
using UnityEngine.Rendering;


namespace KWS
{
    [ExecuteAlways]
    [Serializable]
    [AddComponentMenu("")]
    public partial class WaterSystem : MonoBehaviour
    {
        //todo add KWS_STANDARD/KWS_HDRP/KWS_URP

        [SerializeField] public WaterSystemScriptableData Profile;
        [SerializeField] public WaterSystemScriptableData Settings;

        #region public API methods


        /// <summary>
        /// You must invoke this method every time you change any water parameter.
        /// For example
        /// waterInstance.Settings.Transparent = 5;
        /// waterInstance.Settings.UseVolumetricLighting = false;
        /// waterInstance.ForceUpdateWaterSettings();
        /// 
        /// A faster option is when you indicate which parameters tab has been changed
        /// for example
        /// waterInstance.ForceUpdateWaterSettings(WaterTab.ColorSettings | WaterTab.VolumetricLighting);
        /// </summary>
        public void ForceUpdateWaterSettings(WaterTab changedTab = WaterTab.All)
        {
            UpdateState(changedTab);
        }

        /// <summary>
        /// You can manually control the water rendering.
        /// For example, you can disable rendering if you go into a cave, etc
        /// </summary>
        public bool WaterRenderingActive = true;

        /// <summary>
        /// Called when visible water is visible and rendered
        /// </summary>
        public Action OnWaterRender;

        /// <summary>
        /// World space bounds of the rendered mesh (the box size relative to the wind speed).
        /// </summary>
        /// <returns></returns>
        public Bounds WorldSpaceBounds;
        
        /// <summary>
        /// Check if the current camera is under water. This method does not use an accurate calculation of waves, but instead uses approximation, so sometimes with high waves it return true.
        /// </summary>
        public static bool IsCameraUnderwater { get; private set; }

        public static bool IsSphereUnderWater(Vector3 centerWorldPos, float radius)
        {

            if (IsPositionUnderWater(centerWorldPos - Vector3.one * radius)) return true;
            if (IsPositionUnderWater(centerWorldPos + Vector3.up * radius)) return true;

            if (IsPositionUnderWater(centerWorldPos + Vector3.back * radius)) return true;
            if (IsPositionUnderWater(centerWorldPos + Vector3.forward * radius)) return true;

            if (IsPositionUnderWater(centerWorldPos + Vector3.left * radius)) return true;
            if (IsPositionUnderWater(centerWorldPos + Vector3.right * radius)) return true;

            return false;
        }

        /// <summary>
        /// Check if the current world space position is under water. For example, you can detect if your character enters the water to like triggering a swimming state.
        /// </summary>
        /// <param name="worldPos"></param>
        /// <returns></returns>
        public static bool IsPositionUnderWater(Vector3 worldPos)
        {
            foreach (var instance in WaterSharedResources.WaterInstances)
            {
                if(!instance.IsWaterVisible) continue;
                if (!instance.WorldSpaceBounds.Contains(worldPos)) continue;
                return instance.IsWorldPosUnderwaterAccurate(worldPos);
            }

            return false;
        }

        /// <summary>
        /// Get world space water position/normal at point. Used for water physics. It check all water instances. 
        /// </summary>
        /// <param name="worldPosition"></param>
        /// <returns></returns>
        public static WaterSurfaceData GetWaterSurfaceData(Vector3 worldPosition)
        {
            foreach (var instance in WaterSharedResources.WaterInstances)
            {
                if (!instance.IsWaterVisible && UseNetworkBuoyancy == false) continue;
                if (!instance.WorldSpaceBounds.Contains(worldPosition)) continue;
                return instance.GetCurrentWaterSurfaceData(worldPosition);
            }
            return BuoyancyPass.GetDefault(worldPosition, worldPosition.y);
        }

        /// <summary>
        /// Get world space water position/normal at point. Used for water physics. It check all instances
        /// </summary>
        /// <param name="worldPositions">World positions</param>
        /// <param name="worldNormals">Default up world vectors, used only as container to initialize the array, you can pass any values</param>
        /// <returns></returns>
        public static void GetWaterSurfaceDataArray(Vector3[] worldPositions, Vector3[] worldNormals)
        {
            foreach (var instance in WaterSharedResources.WaterInstances)
            {
                if (!instance.IsWaterVisible && UseNetworkBuoyancy == false) continue;
                if (!instance.WorldSpaceBounds.Contains(worldPositions[0])) continue;

                instance.GetCurrentWaterSurfaceData(worldPositions, worldNormals);
            }
        }

        /// <summary>
        /// Returns the height of the first found water
        /// </summary>
        /// <param name="worldPosition"></param>
        /// <returns></returns>
        public static float FindWaterLevelAtLocation(Vector3 worldPosition)
        {
            foreach (var instance in WaterSharedResources.WaterInstances)
            {
                if (instance.WorldSpaceBounds.ContainsXZ(worldPosition)) return instance.GetCurrentWaterSurfaceHeightFast(worldPosition);
            }

            return worldPosition.y;
        }

        /// <summary>
        /// Activate this option if you want to manually synchronize the time for all clients over the network
        /// </summary>
        public static bool UseNetworkTime;
        public static float NetworkTime;
        public static bool  UseNetworkBuoyancy;

        public Mesh SplineRiverMesh { get; private set; }


        #endregion

        #region editor variables
        internal Action<WaterSystem, WaterTab> OnWaterSettingsChanged;
       
        [SerializeField] internal bool ShowColorSettings = true;
        [SerializeField] internal bool ShowExpertColorSettings = false;

        [SerializeField] internal bool ShowWaves = true;
        [SerializeField] internal bool ShowExpertWavesSettings = false;

        [SerializeField] internal bool ShowFoam = false;

        [SerializeField] internal bool ShowReflectionSettings = false;
        [SerializeField] internal bool ShowExpertReflectionSettings = false;

        [SerializeField] internal bool ShowRefractionSettings = false;
        [SerializeField] internal bool ShowExpertRefractionSettings = false;

        [SerializeField] internal bool ShowVolumetricLightSettings = false;
        [SerializeField] internal bool ShowExpertVolumetricLightSettings = false;

        [SerializeField] internal bool ShowFlowMap = false;
        [SerializeField] internal bool ShowExpertFlowmapSettings = false;
        [SerializeField] internal bool FlowMapInEditMode = false;
        [SerializeField] internal float FlowMapBrushStrength = 0.75f;

        [SerializeField] internal bool ShowDynamicWaves = false;
        [SerializeField] internal bool ShowExpertDynamicWavesSettings = false;

        [SerializeField] internal bool ShowShorelineMap = false;
        [SerializeField] internal bool ShorelineInEditMode = false;
        [SerializeField] internal bool ShowExpertShorelineSettings = false;

        [SerializeField] internal bool ShowFoamSettings = false;

        [SerializeField] internal bool ShowCausticEffectSettings = false;
        [SerializeField] internal bool ShowExpertCausticEffectSettings = false;
        [SerializeField] internal bool CausticDepthScaleInEditMode = false;

        [SerializeField] internal bool ShowUnderwaterEffectSettings = false;
        [SerializeField] internal bool ShowExpertUnderwaterSettings = false;

        [SerializeField] internal bool ShowMeshSettings             = false;
        [SerializeField] internal bool SplineMeshInEditMode         = false;
        [SerializeField] internal bool ShowExpertMeshSettings       = false;

        [SerializeField] internal bool WideAngleCameraRenderingMode = false;

        internal static bool DebugQuadtree      = false;
        internal        bool DebugAABB          = false;
        internal        bool DebugBuoyancy      = false;
        internal        bool DebugFft           = false;
        internal        bool DebugOrthoDepth    = false;
        internal        bool DebugDynamicWaves  = false;
        internal static bool DebugUpdateManager = false;

        internal static bool RebuildMaxHeight  = false;

        [SerializeField] internal bool ShowRendering = false;
        [SerializeField] internal bool ShowExpertRenderingSettings = false;

        [SerializeField] internal static int SelectedThirdPartyFogMethod = -1;

        [Serializable]
        internal class ThirdPartyAssetDescription
        {
            public string EditorName;
            public string ShaderDefine           = String.Empty;
            public string ShaderInclude          = String.Empty;
            public string AssetNameSearchPattern = String.Empty;
            public bool   DrawToDepth;
            public int    CustomQueueOffset;
            public bool   IgnoreInclude;
            public bool   OverrideNativeCubemap;
        }

#if UNITY_EDITOR
        internal KWS_EditorShoreline  _shorelineEditor = new KWS_EditorShoreline();
        internal KWS_EditorFlowmap    _flowmapEditor   = new KWS_EditorFlowmap();
        internal KWS_EditorSplineMesh _splineMeshEditor;
#endif


#endregion

        #region internal variables

        internal WaterInstanceResources InstanceData = new WaterInstanceResources();
        internal WaterSurfaceData SurfaceData = new WaterSurfaceData();

        public string WaterInstanceID
        {
            get
            {
#if UNITY_EDITOR
                if (string.IsNullOrEmpty(_waterGUID)) _waterGUID = CreateWaterInstanceID();
                return _waterGUID;
#else
                if (string.IsNullOrEmpty(_waterGUID)) Debug.LogError("Water GUID is empty, therefore shoreline/flowing/etc will not work!");
                return _waterGUID;
#endif
            }
        }


        string CreateWaterInstanceID()
        {
#if UNITY_EDITOR
            return KWS_EditorUtils.GetNormalizedSceneName() + "." + Path.GetRandomFileName().Substring(0, 5).ToUpper();
#else
            return string.Empty;
#endif
        }


        internal Vector3 WaterRelativeWorldPosition
        {
            get
            {
                if (Settings.WaterMeshType == WaterMeshTypeEnum.InfiniteOcean)
                {
                    var pos = _currentCamera.GetCameraPositionFast();
                    pos.y = WaterPivotWorldPosition.y;
                    return pos;
                }
                else return WaterPivotWorldPosition;
            }
        }

        internal Quaternion WaterPivotWorldRotation;
        internal Vector3    WaterPivotWorldPosition;
        internal float    WaterBoundsSurfaceHeight;
        internal Vector3    WaterSize;

        private Transform _waterTransform;
        internal Transform WaterRootTransform
        {
            get
            {
                if (_waterTransform == null) _waterTransform = transform;
                return _waterTransform;
            }
        }

        internal bool IsWaterVisible { get; private set; }
        internal bool IsCameraUnderwaterForInstance;
        internal bool CanRenderTesselation => Settings.UseTesselation && SystemInfo.graphicsShaderLevel >= 46 && SplineMeshInEditMode == false;
        internal float CurrentMaxWaveHeight;
        internal float SkyLodRelativeToWind => Mathf.Lerp(0.25f, KWS_Settings.Reflection.MaxSkyLodAtFarDistance, Mathf.Pow(Mathf.Clamp01(Settings.CurrentWindSpeed / 15f), 0.35f));
        
        internal int                     WaterShaderPassID;
        internal ScriptableRenderContext CurrentContext;
        internal bool                    IsFluidsRenderedInTheCurrentFrame;

        public WaterSurfaceData GetCurrentWaterSurfaceData(Vector3 worldPosition)
        {
            if (Settings.UseGlobalWind) WaterSharedResources.GlobalWindBuoyancyLeftFramesAfterRequest = 0;
            else InstanceData.BuoyancyLeftFramesAfterRequest                                          = 0;

            if (Settings.WaterMeshType == WaterMeshTypeEnum.River)
            {
                if (SplineMeshComponent.GetSplineSurfaceHeight(this, worldPosition, out var surfaceWorldPos, out var surfaceNormal))
                {
                    var data = BuoyancyPass.GetWaterSurfaceData(this, surfaceWorldPos);
                    if (data.IsActualDataReady)
                    {
                        data.Position.y = surfaceWorldPos.y + data.Position.y - WaterPivotWorldPosition.y;
                        data.Normal     = KW_Extensions.BlendNormals(data.Normal, surfaceNormal);
                        return data;
                    }
                }
            }
            else
            {
                return BuoyancyPass.GetWaterSurfaceData(this, worldPosition);
            }

            return BuoyancyPass.GetDefault(worldPosition, WaterBoundsSurfaceHeight);
        }

        public void GetCurrentWaterSurfaceData(Vector3[] worldPositions, Vector3[] worldNormals)
        {
            if (Settings.UseGlobalWind) WaterSharedResources.GlobalWindBuoyancyLeftFramesAfterRequest = 0;
            else InstanceData.BuoyancyLeftFramesAfterRequest                                          = 0;

            if (Settings.WaterMeshType == WaterMeshTypeEnum.River)
            {
                SplineMeshComponent.GetSplineSurfaceHeights(this, worldPositions, out var surfaceHeights, out var surfaceNormals);
                BuoyancyPass.GetWaterSurfaceData(this, worldPositions, worldNormals, 0);

                for (int i = 0; i < worldPositions.Length; i++)
                {
                    worldPositions[i].y = surfaceHeights[i] + worldPositions[i].y;
                    worldNormals[i]     = KW_Extensions.BlendNormals(surfaceNormals[i], worldNormals[i]);
                }
            }
            else
            { 
                //todo add better support for custom meshes with different pivot
                var surfaceHeight = Settings.WaterMeshType == WaterSystemScriptableData.WaterMeshTypeEnum.CustomMesh ? WaterBoundsSurfaceHeight : WaterPivotWorldPosition.y;
                BuoyancyPass.GetWaterSurfaceData(this, worldPositions, worldNormals, surfaceHeight);
            }
        }

        #endregion

        #region private variables

        [SerializeField] string _waterGUID;

#if KWS_DEBUG
        public static Vector4 Test4 = Vector4.zero;
        public static float VRScale = 1;
#endif

        internal static GameObject                     UpdateManagerObject;
        internal static Camera                         _currentCamera;

        Renderer                       _aabbRenderer;
        MeshFilter                       _aabbMeshFilter;
        private GameObject             _aabbRendererGameObject;
        List<ReflectionProbeBlendInfo> _afectedProbes = new List<ReflectionProbeBlendInfo>();

        private bool                   _isSceneCameraRendered;

        internal bool IsWaterInitialized { get; private set; }
        private bool isWaterPlatformSpecificResourcesInitialized;

        #endregion

        #region properties

        UndoProvider _undoProvider;
        internal UndoProvider UndoProvider
        {
            get
            {
                if (_undoProvider == null && UpdateManagerObject != null)
                {
                    _undoProvider           = gameObject.AddComponent<UndoProvider>();
                    _undoProvider.hideFlags = HideFlags.HideInInspector;
                }
               

                return _undoProvider;
            }
        }

        internal MeshQuadTree _meshQuadTree = new MeshQuadTree();
        internal KWS_SplineMesh SplineMeshComponent = new KWS_SplineMesh();

        #endregion

        private GameObject _infiniteOceanShadowCullingAnchor;
        private Transform _infiniteOceanShadowCullingAnchorTransform;


        internal static Dictionary<Camera, CameraData> CameraDatas = new Dictionary<Camera, CameraData>();
        internal static Camera _lastGameCamera;
        internal static Camera _lastEditorCamera;


        internal class CameraData
        {
            public Transform CameraTransform;
            public Vector3 Position;
            public Vector3 LastPosition;
            public Vector3 RotationEuler;
            public Vector3 Forward;
            public CameraData(Camera camera)
            {
                CameraTransform = camera.transform;
            }

            public void Update()
            {
                LastPosition = Position;
                Position = CameraTransform.position;
                RotationEuler = CameraTransform.rotation.eulerAngles;
                Forward = CameraTransform.forward;
            }
        }


        void CheckPlatformSpecificFeatures()
        {
            if (!KWS_CoreUtils.IsAtomicsSupported()) Settings.UseShorelineFoamFastMode = false;
        }


        internal void LoadOrCreateSettings()
        {
            if (Settings == null)
            {
                Settings = (Profile != null) 
                    ? ScriptableObject.Instantiate(Profile) 
                    : ScriptableObject.CreateInstance<WaterSystemScriptableData>();
            }
        }

        private void Awake()
        {
            LoadOrCreateSettings();
        }

        private void OnEnable()
        {
            WaterSharedResources.OnWaterInstancesCountChanged += OnWaterInstancesCountChangedEvent;
            AddWaterInstance();

            WaterSharedResources.UpdateReflectionProbeCache();

            if (UpdateManagerObject == null)
            {
                UpdateManagerObject = KW_Extensions.CreateHiddenGameObject("KWS_UpdateManager");
                UpdateManagerObject.AddComponent<KWS_UpdateManager>();
            }

            if (_aabbRendererGameObject == null)
            {
                _aabbRendererGameObject                         = KW_Extensions.CreateHiddenGameObject("KWS_ReflectionProbeBoundsMesh");
                _aabbRendererGameObject.transform.parent        = WaterRootTransform;
                _aabbRendererGameObject.transform.localPosition = Vector3.zero;
                _aabbRendererGameObject.transform.localScale = Vector3.one;
                _aabbRendererGameObject.transform.localRotation = Quaternion.identity;

                _aabbRenderer         = _aabbRendererGameObject.GetOrAddComponent<MeshRenderer>();
                _aabbRenderer.enabled = false;

                _aabbMeshFilter = _aabbRendererGameObject.GetOrAddComponent<MeshFilter>();
            }
          
            //if (CubemapReflectionComponent == null) CubemapReflectionComponent = new CubemapReflection(this);
            PlanarReflectionComponent.OnEnable();

            OnWaterSettingsChanged                     += OnCurrentWaterSettingsChangedEvent;

            LoadOrCreateSettings();
            CheckCopyPastAndReplaceInstanceID();

            InstanceData.Initialize(this);
            InstanceData.InitializeMaterials();

            CheckPlatformSpecificFeatures();
            SetGlobalShaderParams();

            UpdateWaterInstance(WaterTab.All);
        }

        void OnDestroy()
        {
            if(IsWaterInitialized) OnDisable();
        }

        void OnDisable()
        {
            WaterSharedResources.OnWaterInstancesCountChanged -= OnWaterInstancesCountChangedEvent;
            RemoveWaterInstance();

            if (WaterSharedResources.WaterInstances.Count == 0)
            {
                KW_Extensions.SafeDestroy(UpdateManagerObject);
                UpdateManagerObject = null;
            }

            OnWaterSettingsChanged                     -= OnCurrentWaterSettingsChangedEvent;

            if (WaterSharedResources.WaterInstances.Count == 0 && WaterSharedResources.KWS_OceanFoamTex != null)
            {
                Resources.UnloadAsset(WaterSharedResources.KWS_OceanFoamTex);
                WaterSharedResources.KWS_OceanFoamTex = null;
            }

            if (WaterSharedResources.WaterInstances.Count == 0 && WaterSharedResources.KWS_IntersectionFoamTex != null)
            {
                Resources.UnloadAsset(WaterSharedResources.KWS_IntersectionFoamTex);
                WaterSharedResources.KWS_IntersectionFoamTex = null;
            }

            if (WaterSharedResources.WaterInstances.Count == 0 && WaterSharedResources.KWS_FluidsFoamTex != null)
            {
                Resources.UnloadAsset(WaterSharedResources.KWS_FluidsFoamTex);
                WaterSharedResources.KWS_FluidsFoamTex = null;
            }

            ReleasePlatformSpecificResources();
            Release();
        }
        
        internal void OnPreWaterRender(Camera cam)
        {
            //Debug.Log("UpdatePerWaterInstance " + cam.name);
            _isSceneCameraRendered = _currentCamera.cameraType == CameraType.SceneView;
           
           
            //ScreenSpaceBounds = KW_Extensions.GetScreenSpaceBounds(cam, WorldSpaceBounds);
            //Debug.Log(ScreenSpaceBounds);

            Profiler.BeginSample("Water.Rendering");
            OnWaterRender?.Invoke();
            CheckAndUpdateShaderParams();
            RenderWater();
            Profiler.EndSample();
        }

        

        internal static void OnPreCameraRender(Camera cam)
        {
            //Debug.Log(" UpdatePerCamera " + cam);
            OverrideCameraRequiredSettings(cam);

#if KWS_DEBUG
            if (DebugQuadtree && cam.cameraType != CameraType.Game) return;
#endif
            _currentCamera = cam;
          

            if (cam.cameraType == CameraType.Game) _lastGameCamera = cam;
            else if (cam.cameraType == CameraType.SceneView) _lastEditorCamera = cam;
           
            if (CameraDatas.Count > 100) CameraDatas.Clear();
            if (!CameraDatas.ContainsKey(cam)) CameraDatas.Add(cam, new CameraData(cam));
            CameraDatas[cam].Update();

            if (WaterSharedResources.FrustumCaches.Count > 10) WaterSharedResources.FrustumCaches.Clear();
            if (!WaterSharedResources.FrustumCaches.ContainsKey(cam)) WaterSharedResources.FrustumCaches.Add(cam, new WaterSharedResources.CameraFrustumCache());
            WaterSharedResources.FrustumCaches[cam].Update(cam);

            UpdateWaterVisibilityForAllInstances();
            UpdateUnderwaterStateForAllInstances(cam);
        
#if KWS_DEBUG
            Shader.SetGlobalVector("Test4", Test4);
            if (KWS_CoreUtils.SinglePassStereoEnabled) UnityEngine.XR.XRSettings.eyeTextureResolutionScale = VRScale;
#endif
            SetGlobalCameraShaderParams(cam);
            SetGlobalPlatformSpecificShaderParams(cam);
            UpdateArrayShaderParams();

        }


        void Release()
        {
            IsWaterInitialized = false;

            _meshQuadTree.Release();
            SplineMeshComponent.Release();
            PlanarReflectionComponent.Release();

            if (_infiniteOceanShadowCullingAnchor != null) KW_Extensions.SafeDestroy(_infiniteOceanShadowCullingAnchor.GetComponent<MeshFilter>().sharedMesh,
                                                                                     _infiniteOceanShadowCullingAnchor.GetComponent<MeshRenderer>().sharedMaterial);

            KW_Extensions.SafeDestroy(_infiniteOceanShadowCullingAnchor);
            if (Settings.WaterMeshType != WaterMeshTypeEnum.CustomMesh) KW_Extensions.SafeDestroy(SplineRiverMesh);

            _isFluidsSimBakedMode = false;

            isWaterPlatformSpecificResourcesInitialized = false;
            IsWaterVisible = false;

            WaterShaderPassID = 0;

            InstanceData.Release();
            CameraDatas.Clear();
#if KWS_DEBUG
            DebugHelpers.Release();
#endif

#if UNITY_EDITOR
            _flowmapEditor.Release();
#endif
            
        }
    }
}