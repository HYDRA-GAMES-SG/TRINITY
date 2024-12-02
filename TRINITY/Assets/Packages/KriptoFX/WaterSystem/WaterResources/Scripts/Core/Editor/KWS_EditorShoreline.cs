#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static KWS.ShorelineWavesScriptableData;
using Random = UnityEngine.Random;

namespace KWS
{
    internal class KWS_EditorShoreline
    {
        bool  _isMousePressed            = false;
        int   _nearMouseSelectionWaveIdx = -1;

        const float minWaveScale = 0.35f;
        const float maxWaveScale = 1.75f;
        private readonly Vector3 MinWaveSize = DefaultWaveSize * minWaveScale;
        private readonly Vector3 MaxWaveSize = DefaultWaveSize * maxWaveScale;

        private static readonly Vector3 DefaultWaveSize = new Vector3(14, 4.5f, 16);

        private readonly Color _waveColorSolidBox = new Color(0, 0.75f, 0.99f, 0.01f);
        private readonly Color _selectedWaveColorSolidBox = new Color(0, 0.75f, 0.99f, 0.1f);

        private readonly Color _waveColorWireBox = new Color(0, 0.75f, 0.99f, 0.2f);
        private readonly Color _selectedWaveColorWireBox  = new Color(0, 0.75f, 0.99f, 0.9f);

        public void AddWave(WaterSystem waterSystem, Ray ray, bool interpolateNextPosition)
        {
            var waves = waterSystem.GetShorelineWaves();
            var newWave = InitializeNewWave(waves, ray, waterSystem.WaterPivotWorldPosition.y, interpolateNextPosition);
            waves.Add(newWave);
        }

        public void RemoveAllWaves(WaterSystem waterSystem)
        {
            var waves = waterSystem.GetShorelineWaves();
            waves.Clear();
        }

        ShorelineWave InitializeNewWave(List<ShorelineWave> waves, Ray ray, float waterHeight, bool interpolateNextPosition)
        {
            var position = GetWaterRayIntersection(ray, waterHeight);
            float rotation = 0;
            var scale = Vector3.one;
            var timeOffset = GetShorelineTimeOffset(waves);
            var randomFlip = Random.Range(0f, 1f) > 0.5f;

            if (waves.Count > 0)
            {
                var lastIdx = waves.Count - 1;
                rotation = waves[lastIdx].EulerRotationY;
                
                scale *= Random.Range(0.75f, 1.25f);
                
                if (interpolateNextPosition)
                {
                    if (waves.Count < 2) position.x += 10;
                    else
                    {
                        var direction = (waves[lastIdx].Position - waves[lastIdx - 1].Position).normalized;
                        position = waves[lastIdx].Position + waves[lastIdx].Scale.magnitude * direction;
                    }
                }
            }

            position.y = waterHeight;
            return new ShorelineWave(0, position, rotation, scale, timeOffset, randomFlip);
        }

        public Ray GetCurrentMouseToWorldRay()
        {
            return HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        }


        public Vector3 GetSceneCameraPosition()
        {
            var sceneCamera = KWS_EditorUtils.GetSceneCamera();
             return sceneCamera.transform.position;
        }


        public Ray GetCameraToWorldRay()
        {
            var sceneCamT = KWS_EditorUtils.GetSceneCamera().transform;
            return new Ray(sceneCamT.position, sceneCamT.forward * 1000);
        }

        public Vector3 GetWaterRayIntersection(Ray ray, float waterHeight)
        {
            var plane = new Plane(Vector3.down, waterHeight);

            if (plane.Raycast(ray, out var distanceToPlane))
            {
                return ray.GetPoint(distanceToPlane);
            }
            return Vector3.zero;
        }

        public float GetShorelineTimeOffset(List<ShorelineWave> wavesData)
        {
            if (wavesData.Count == 0) return 0;
            var timeOffset = wavesData[wavesData.Count - 1].TimeOffset + Random.Range(0.13f, 0.22f);
            return timeOffset % 1;
        }

        public int GetWaveNearestToMouse(WaterSystem waterSystem, List<ShorelineWave> waves)
        {
            var   mouseWorldPos = KWS_EditorUtils.GetMouseWorldPosProjectedToWaterPlane(waterSystem.WaterPivotWorldPosition.y, Event.current);
            float minDistance   = float.PositiveInfinity;
            int   minIdx        = 0;
            if (!float.IsInfinity(mouseWorldPos.x))
            {
                for (var i = 0; i < waves.Count; i++)
                {
                    var wave        = waves[i];
                    var distToMouse = new Vector2(wave.Position.x - mouseWorldPos.x, wave.Position.z - mouseWorldPos.z).magnitude;
                    var waveRadius  = new Vector2(wave.Size.x * wave.Scale.x, wave.Size.z * wave.Scale.z).magnitude * 2.0f;

                    if (distToMouse < waveRadius && distToMouse < minDistance)
                    {
                        minDistance = distToMouse;
                        minIdx      = i;
                    }
                }
            }

            return minIdx;
        }

        internal void SaveShorelineWavesData(WaterSystem waterInstance)
        {
#if UNITY_EDITOR
            waterInstance.Settings.ShorelineWavesScriptableData = waterInstance.Settings.ShorelineWavesScriptableData.SaveScriptableData(waterInstance.WaterInstanceID, null, "ShorelineWavesProfile");
#endif
        }


        public void DrawShorelineEditor(WaterSystem waterSystem)
        {
            if (Application.isPlaying) return;
            var waves = waterSystem.GetShorelineWaves();
            if (waves == null || waves.Count == 0) return;

            waterSystem.UndoProvider.ShorelineWaves = waves;
            Undo.RecordObject(waterSystem.UndoProvider, "Changed shoreline");

            var defaultLighting = Handles.lighting;
            var defaultZTest = Handles.zTest;
            var defaultMatrix = Handles.matrix;

            Handles.lighting = false;
            Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;

            var e = Event.current;

            if (e.type == EventType.MouseDown) _isMousePressed = true;
            else if (e.type == EventType.MouseUp) _isMousePressed = false;
            
            if (!_isMousePressed) _nearMouseSelectionWaveIdx = GetWaveNearestToMouse(waterSystem, waves);
           
            int controlID = GUIUtility.GetControlID(FocusType.Passive);
            if (Event.current.GetTypeForControl(controlID) == EventType.KeyDown)
            {
                if (Event.current.keyCode == KeyCode.Insert)
                {
                    AddWave(waterSystem, GetCurrentMouseToWorldRay(), false);
                }

                if (Event.current.keyCode == KeyCode.Delete)
                {
                    waves.RemoveAt(_nearMouseSelectionWaveIdx);
                    Event.current.Use();
                }
            }
          
            var waterYPos = waterSystem.WaterPivotWorldPosition.y;
            for (var i = 0; i < waves.Count; i++)
            {
                var wave = waves[i];
                Handles.matrix = defaultMatrix;

                if (_nearMouseSelectionWaveIdx == i)
                {
                    switch (Tools.current)
                    {
                        case Tool.Move:
                            var newWavePos = Handles.DoPositionHandle(wave.Position, Quaternion.identity);
                            newWavePos.y = waterYPos;
                            if (wave.Position != newWavePos)
                            {
                                wave.Position = newWavePos;
                                wave.UpdateMatrix();
                            }
                            
                            break;

                        case Tool.Rotate:
                            {
                                var currentRotation = Quaternion.Euler(0, wave.EulerRotationY, 0);
                                var newRotation = Handles.DoRotationHandle(currentRotation, wave.Position);
                                wave.EulerRotationY = newRotation.eulerAngles.y;
                                wave.UpdateMatrix();
                                break;
                            }
                        case Tool.Scale:
                            {
                                var distToCamera = Vector3.Distance(GetSceneCameraPosition(), wave.Position);
                                var handleScaleToCamera = Mathf.Lerp(1, 50, Mathf.Clamp01(distToCamera / 500));

                                var currentSize = Vector3.Scale(wave.Size, wave.Scale);
                                var newSize = Handles.DoScaleHandle(currentSize, wave.Position, Quaternion.Euler(0, wave.EulerRotationY, 0), handleScaleToCamera);

                                if (currentSize != newSize)
                                {
                                    newSize = KW_Extensions.ClampVector3(newSize, MinWaveSize, MaxWaveSize);
                                    wave.Scale = new Vector3(newSize.x / wave.Size.x, newSize.y / wave.Size.y, newSize.z / wave.Size.z);
                                    wave.UpdateMatrix();
                                }
                                break;
                            }
                    }
                }

                waves[i] = wave;

                Handles.color = _nearMouseSelectionWaveIdx == i ? _selectedWaveColorWireBox : _waveColorWireBox;
                Handles.matrix = Matrix4x4.TRS(wave.Position, Quaternion.Euler(0, wave.EulerRotationY, 0), Vector3.Scale(wave.Size, wave.Scale));
                Handles.DrawWireCube(Vector3.zero, Vector3.one);

                Handles.color = _nearMouseSelectionWaveIdx == i ? _selectedWaveColorSolidBox : _waveColorSolidBox;
                Handles.CubeHandleCap(0, Vector3.zero, Quaternion.identity, 1, EventType.Repaint);

            }

            Handles.matrix = defaultMatrix;
            Handles.lighting = defaultLighting;
            Handles.zTest = defaultZTest;

            KWS_EditorUtils.LockLeftClickSelection(controlID);

           
        }
    }
}

#endif