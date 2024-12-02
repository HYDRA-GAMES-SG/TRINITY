#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using static KWS.KWS_CoreUtils;

namespace KWS
{
    internal class KWS_EditorFlowmap
    {
        private float floatMapCircleRadiusDefault = 2f;
        private bool leftKeyPressed;
        private Vector3 flowMapLastPos = Vector3.positiveInfinity;


        private Material _flowMaterial;

        public RenderTexture _flowmapRT;
        public RenderTexture _tempRT;
        //public  Texture2D     _flowmapTex;
        private Texture2D _grayTex;

        private int _currentAreaSize;


        private Material FlowMaterial
        {
            get
            {
                if (_flowMaterial == null) _flowMaterial = CreateMaterial(KWS_ShaderConstants.ShaderNames.FlowMapShaderName);
                return _flowMaterial;
            }
        }

        public void Release()
        {
            KW_Extensions.SafeDestroy(_flowmapRT, _tempRT);
            _flowmapRT = null;
            _tempRT = null;
            KW_Extensions.SafeDestroy(_flowMaterial, _grayTex);
        }

        public void ClearFlowMap(WaterSystem _waterInstance)
        {
#if UNITY_EDITOR
            ClearRenderTexture(_flowmapRT, ClearFlag.Color, new Color(0.5f, 0.5f, 0.5f, 0.5f));
            _waterInstance.InstanceData.Flowmap = _flowmapRT;

            var pathToSceneFolder = KW_Extensions.GetPathToSceneInstanceFolder();
            var pathToFile        = Path.Combine(pathToSceneFolder, KWS_Settings.DataPaths.FlowmapTexture);
            pathToFile = pathToFile + ".png";
            if (!File.Exists(pathToFile)) return;

            UnityEditor.AssetDatabase.DeleteAsset(pathToFile);
#endif
        }


        private void InitializeFlowmapRT(int size)
        {
            _flowmapRT = new RenderTexture(size, size, 0, GraphicsFormat.R16G16_SFloat) { name = "_FlowmapRT" };
            ClearRenderTexture(_flowmapRT, ClearFlag.Color, new Color(0.5f, 0.5f, 0.5f, 0.5f));
        }


        internal void LoadFlowMap(WaterSystem _waterInstance)
        {
            var data = _waterInstance.Settings.FlowingScriptableData;
            if (data == null || data.FlowmapTexture == null) return;

            _waterInstance.Settings.FlowMapTextureResolution = (WaterSystemScriptableData.FlowmapTextureResolutionEnum)data.FlowmapResolution;
            _waterInstance.Settings.FlowMapAreaSize = data.AreaSize;
            _waterInstance.InstanceData.Flowmap = data.FlowmapTexture;
        }


        public void InitializeFlowMapEditor(WaterSystem _waterInstance)
        {
            LoadFlowMap(_waterInstance);

            var textureSize = (int)_waterInstance.Settings.FlowMapTextureResolution;
            var areaSize = _waterInstance.Settings.FlowMapAreaSize;

            if (_flowmapRT == null)
            {
                InitializeFlowmapRT(textureSize);

                if (_tempRT == null) _tempRT = new RenderTexture(textureSize, textureSize, 0, _flowmapRT.graphicsFormat) { name = "_tempFlowmapRT" };

                var flowTex = _waterInstance.Settings.FlowingScriptableData != null && _waterInstance.Settings.FlowingScriptableData.FlowmapTexture != null
                    ? _waterInstance.Settings.FlowingScriptableData.FlowmapTexture
                    : null;
                if (flowTex != null)
                {
                    var activeRT = RenderTexture.active;
                    Graphics.Blit(flowTex, _flowmapRT);
                    RenderTexture.active = activeRT;
                }

                _waterInstance.InstanceData.Flowmap = _flowmapRT;
                _currentAreaSize = areaSize;
            }

        }


        CommandBuffer _cmd;
        public void DrawOnFlowMap(WaterSystem _waterInstance, Vector3 brushPosition, Vector3 brushMoveDirection, float circleRadius, float brushStrength, bool eraseMode = false)
        {
            InitializeFlowMapEditor(_waterInstance);

            var brushSize = _currentAreaSize / circleRadius;
            var uv = new Vector2(brushPosition.x / _currentAreaSize + 0.5f, brushPosition.z / _currentAreaSize + 0.5f);
            if (brushMoveDirection.magnitude < 0.001f) brushMoveDirection = Vector3.zero;

            FlowMaterial.SetVector("_MousePos", uv);
            FlowMaterial.SetVector("_Direction", new Vector2(brushMoveDirection.x, brushMoveDirection.z));
            FlowMaterial.SetFloat("_Size", brushSize * 0.75f);
            FlowMaterial.SetFloat("_BrushStrength", brushStrength / (circleRadius * 3));
            FlowMaterial.SetInt("isErase", eraseMode ? 1 : 0);

            if(_cmd == null) _cmd = new CommandBuffer() {name = "_flowMapEditor"};
            _cmd.Clear();
            _cmd.Blit(_flowmapRT, _tempRT, FlowMaterial, 0);
            _cmd.Blit(_tempRT, _flowmapRT);
            Graphics.ExecuteCommandBuffer(_cmd);
            _waterInstance.InstanceData.Flowmap = _flowmapRT;

        }

        public void SaveFlowMap(WaterSystem _waterInstance)
        {
            int areaSize = _waterInstance.Settings.FlowMapAreaSize;
            Vector3 areaPos = _waterInstance.Settings.FlowMapAreaPosition;
            int resolution = (int)_waterInstance.Settings.FlowMapTextureResolution;
            string _waterInstanceID = _waterInstance.WaterInstanceID;
#if UNITY_EDITOR
            var pathToSceneFolder = KW_Extensions.GetPathToSceneInstanceFolder();
            var pathToFile        = Path.Combine(pathToSceneFolder, KWS_Settings.DataPaths.FlowmapTexture);
            _flowmapRT.SaveRenderTexture(pathToFile, useAutomaticCompressionFormat: true, KW_Extensions.UsedChannels._RG, isHDR: false, mipChain: false);

            var data = _waterInstance.Settings.FlowingScriptableData;
            if(data == null) data = ScriptableObject.CreateInstance<FlowingScriptableData>();
            data.FlowmapTexture                           = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(pathToFile + ".kwsTexture");
            data.AreaSize                                 = areaSize;
            data.AreaPosition                             = areaPos;
            data.FlowmapResolution                        = resolution;
            _waterInstance.Settings.FlowingScriptableData = data.SaveScriptableData(_waterInstanceID, null, "FlowProfile");
#else
            Debug.LogError("You can't save waves data in runtime");
            return null;
#endif
        }

        public void ChangeFlowmapResolution(WaterSystem _waterInstance, int newResolution)
        {
            InitializeFlowMapEditor(_waterInstance);

            KW_Extensions.SafeDestroy(_flowmapRT);
            InitializeFlowmapRT(newResolution);

            var activeRT = RenderTexture.active;
            Graphics.Blit(_tempRT, _flowmapRT);
            RenderTexture.active = activeRT;

            _waterInstance.InstanceData.Flowmap = _flowmapRT;
        }

        public void RedrawFlowMap(WaterSystem _waterInstance, int newAreaSize)
        {
            InitializeFlowMapEditor(_waterInstance);

            var uvScale = (float)newAreaSize / _currentAreaSize;
            _currentAreaSize = newAreaSize;
            FlowMaterial.SetFloat("_UvScale", uvScale);
            var activeRT = RenderTexture.active;
            Graphics.Blit(_flowmapRT, _tempRT, FlowMaterial, 1);
            Graphics.Blit(_tempRT, _flowmapRT);
            RenderTexture.active = activeRT;
        }

        public void DrawFlowMapEditor(WaterSystem _waterInstance, Editor editor)
        {
            if (Application.isPlaying) return;

            var e = Event.current;
            if (e.type == EventType.ScrollWheel)
            {
                floatMapCircleRadiusDefault -= (e.delta.y * floatMapCircleRadiusDefault) / 40f;
                floatMapCircleRadiusDefault = Mathf.Clamp(floatMapCircleRadiusDefault, 0.1f, _waterInstance.Settings.FlowMapAreaSize);
            }

            var controlId = GUIUtility.GetControlID(FocusType.Passive);
            HandleUtility.AddDefaultControl(controlId);
            if (e.type == EventType.ScrollWheel) e.Use();

            var waterPos = _waterInstance.transform.position;
            var waterHeight = _waterInstance.transform.position.y;

            var flowmapWorldPos = _waterInstance.Settings.WaterMeshType == WaterSystemScriptableData.WaterMeshTypeEnum.River ? 
                KWS_EditorUtils.GetMouseWorldPosProjectedToWaterRiver(_waterInstance, e) 
                : KWS_EditorUtils.GetMouseWorldPosProjectedToWaterPlane(waterHeight, e);

            if (float.IsInfinity(flowmapWorldPos.x)) return;
            var flowPosWithOffset = new Vector3(-_waterInstance.Settings.FlowMapAreaPosition.x, 0, -_waterInstance.Settings.FlowMapAreaPosition.z) + (Vector3)flowmapWorldPos;

            Handles.color = e.control ? new Color(1, 0, 0) : new Color(0, 0.8f, 1);
            Handles.CircleHandleCap(controlId, (Vector3)flowmapWorldPos, Quaternion.LookRotation(Vector3.up), floatMapCircleRadiusDefault, EventType.Repaint);

            Handles.color = e.control ? new Color(1, 0, 0, 0.2f) : new Color(0, 0.8f, 1, 0.25f);
            Handles.DrawSolidDisc((Vector3)flowmapWorldPos, Vector3.up, floatMapCircleRadiusDefault);



            // var flowMapAreaPos = new Vector3(waterPos.x + waterSystem.FlowMapOffset.x, waterPos.y, waterPos.z + waterSystem.FlowMapOffset.y);
            var flowMapAreaScale = new Vector3(_waterInstance.Settings.FlowMapAreaSize, 0.5f, _waterInstance.Settings.FlowMapAreaSize);
            Handles.matrix = Matrix4x4.TRS(_waterInstance.Settings.FlowMapAreaPosition, Quaternion.identity, flowMapAreaScale);


            Handles.color = new Color(0, 0.75f, 1, 0.2f);
            Handles.CubeHandleCap(0, Vector3.zero, Quaternion.identity, 1, EventType.Repaint);
            Handles.color = new Color(0, 0.75f, 1, 0.9f);
            Handles.DrawWireCube(Vector3.zero, Vector3.one);

            if (Event.current.button == 0)
            {
                if (e.type == EventType.MouseDown)
                {
                    leftKeyPressed = true;
                    //waterSystem.flowMap.LastDrawFlowMapPosition = flowPosWithOffset;
                }
                if (e.type == EventType.MouseUp)
                {
                    leftKeyPressed = false;
                    flowMapLastPos = Vector3.positiveInfinity;

                    editor.Repaint();
                }
            }

            if (leftKeyPressed)
            {
                if (float.IsPositiveInfinity(flowMapLastPos.x))
                {
                    flowMapLastPos = flowPosWithOffset;
                }
                else
                {
                    var brushDir = (flowPosWithOffset - flowMapLastPos);
                    flowMapLastPos = flowPosWithOffset;
                    DrawOnFlowMap(_waterInstance, flowPosWithOffset, brushDir, floatMapCircleRadiusDefault, _waterInstance.FlowMapBrushStrength, e.control);
                }
            }

        }
    }
}

#endif