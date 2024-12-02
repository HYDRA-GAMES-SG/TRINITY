using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace KWS
{
    internal static class DebugHelpers
    {
        private static Material _debugTextureSliceMaterial;

        public static void DebugFft()
        {
            if (Event.current.type.Equals(EventType.Repaint))
            {
                if (_debugTextureSliceMaterial == null) _debugTextureSliceMaterial = KWS_CoreUtils.CreateMaterial("Hidden/_KriptoFX/KWS/Water/Debug/TextureSlice");
                if (WaterSharedResources.FftWavesDisplacement == null) return;

                var fftRT = WaterSharedResources.FftWavesDisplacement.rt;
                if (fftRT != null)
                {
                    for (int i = 0; i < fftRT.volumeDepth; i++)
                    {
                        _debugTextureSliceMaterial.SetInt("_slice", i);
                        Graphics.DrawTexture(new Rect(i * fftRT.width + i * 5, 0, fftRT.width, fftRT.height), fftRT, _debugTextureSliceMaterial, 0);
                    }
                }

                var fftNormal = WaterSharedResources.FftWavesNormal.rt;
                if (fftNormal != null)
                {
                    for (int i = 0; i < fftNormal.volumeDepth; i++)
                    {
                        _debugTextureSliceMaterial.SetInt("_slice", i);
                        Graphics.DrawTexture(new Rect(i * fftNormal.width + i * 5, fftNormal.height + 5, fftNormal.width, fftNormal.height), fftNormal, _debugTextureSliceMaterial, 1);
                    }
                }

                var debugRT = BuoyancyPass.DebugHeightRT;
                if (debugRT != null && fftRT != null) Graphics.DrawTexture(new Rect(0, fftRT.height * 2 + 10, debugRT.width, debugRT.height), debugRT);
            }
        }

        public static void DebugDynamicWaves()
        {
            if (Event.current.type.Equals(EventType.Repaint))
            {
                var mask   = WaterSharedResources.DynamicWavesMaskRT;
                var target = WaterSharedResources.DynamicWavesRT;
                if (mask != null && target != null)
                {
                    Graphics.DrawTexture(new Rect(0, 0,        512, 512), mask);
                    Graphics.DrawTexture(new Rect(0, 512 + 10, 512, 512), target);
                }
            }
        }

        public static void DebugOrthoDepth()
        {
            if (Event.current.type.Equals(EventType.Repaint))
            {
                var depth   = WaterSharedResources.OrthoDepth;
                if (depth != null)
                {
                    Graphics.DrawTexture(new Rect(0, 0,        512, 512), depth);
                }
            }
        }


        public static void DebugBuoyancy()
        {
            var sizeX        = 20;
            var sizeY        = 20;
            var worldPos     = new Vector3[(sizeX + sizeX) * (sizeY + sizeY)];
            var worldNormals = new Vector3[worldPos.Length];


            /////////////////////////////////////////////////////////////////////////////
            int idx = 0;
            for (int x = -sizeX; x < sizeX; x++)
            {
                for (int z = -sizeY; z < sizeY; z++)
                {
                    worldPos[idx] = new Vector3(x, 0, z) * 2;
                    idx++;
                }
            }

            WaterSystem.GetWaterSurfaceDataArray(worldPos, worldNormals);

            for (int i = 0; i < worldPos.Length; i++)
            {
                Gizmos.DrawCube(worldPos[i], Vector3.one * 0.5f);
                Gizmos.DrawRay(worldPos[i], worldNormals[i]);
            }
            /////////////////////////////////////////////////////////////////////////////



            /////////////////////////////////////////////////////////////////////////////
            idx = 0;
            for (int x = -sizeX; x < sizeX; x++)
            {
                for (int z = -sizeY; z < sizeY; z++)
                {
                    worldPos[idx] = new Vector3(x, 0, z) * 10;
                    idx++;
                }
            }
            WaterSystem.GetWaterSurfaceDataArray(worldPos, worldNormals);

            for (int i = 0; i < worldPos.Length; i++)
            {
                Gizmos.DrawCube(worldPos[i], Vector3.one * 2f);
                Gizmos.DrawRay(worldPos[i], worldNormals[i]);
            }
            /////////////////////////////////////////////////////////////////////////////



            /////////////////////////////////////////////////////////////////////////////
            idx = 0;
            for (int x = -sizeX; x < sizeX; x++)
            {
                for (int z = -sizeY; z < sizeY; z++)
                {
                    worldPos[idx] = new Vector3(x, 0, z) * 50;
                    idx++;
                }
            }
            WaterSystem.GetWaterSurfaceDataArray(worldPos, worldNormals);

            for (int i = 0; i < worldPos.Length; i++)
            {
                Gizmos.DrawCube(worldPos[i], Vector3.one * 4f);
                Gizmos.DrawRay(worldPos[i], worldNormals[i]);
            }
            ///////////////////////////////////////////////////////////////////////////

            //Debug.Log(WaterSystem.GetWaterSurfaceData(new Vector3(10, 0, 10)).Position);
        }

        public static void DebugQuadtree(WaterSystem waterInstance)
        {
            var quadTreeInstance = waterInstance._meshQuadTree.Instances[Camera.main];
            if (waterInstance.Settings.WaterMeshType == WaterSystemScriptableData.WaterMeshTypeEnum.FiniteBox)
            {
                foreach (var visibleNode in quadTreeInstance.VisibleNodes)
                {
                    var center = visibleNode.RotatedCenter;
                    var size   = visibleNode.ChunkSize;
                    center.y -= size.y * 0.5f;
                    Gizmos.DrawWireCube(center, size);
                }
            }
            else if (waterInstance.Settings.WaterMeshType == WaterSystemScriptableData.WaterMeshTypeEnum.InfiniteOcean)
            {
                foreach (var visibleNode in quadTreeInstance.VisibleGPUChunks)
                {
                    var center = visibleNode.Position;
                    var size   = visibleNode.Size;
                    center.y -= size.y * 0.5f;
                    Gizmos.DrawWireCube(center, size);
                }
            }
            Debug.Log($"Visible nodes {quadTreeInstance.VisibleNodes.Count}");
        }
        public static IEnumerator EvaluateMaxAmplitudeTable(WaterSystem waterInstance)
        {
            var buoyancyFps = 1 / 1000f;

            var turbulence = 0f;
            var finalText = "";
            for (int idx = 0; idx <= KWS_Settings.FFT.MaxWindSpeed; idx++)
            {
                waterInstance.Settings.GlobalWindSpeed        = idx;
                waterInstance.Settings.GlobalWindTurbulence = turbulence;
                waterInstance.ForceUpdateWaterSettings();

                float lastMaxHeight = 0;
                int   maxFrames     = (int)Mathf.Lerp(200, 2000, (1f * idx / KWS_Settings.FFT.MaxWindSpeed));
                for (int frame = 0; frame < maxFrames; frame++)
                {
                    UpdateHeight(waterInstance, ref lastMaxHeight);
                    Debug.Log($"idx {idx}  frame {frame}");
                    yield return new WaitForSeconds(buoyancyFps);
                }

                if (idx == 0) lastMaxHeight = 0.001f;
                var maxHeightText           = lastMaxHeight.ToString().Replace(',', '.');

                finalText += $"{idx}, {maxHeightText}" + Environment.NewLine;
            }

            Debug.Log("Finished " + turbulence);
            File.WriteAllText($@"D:\DebugWaterAmplitude_{waterInstance.Settings.CurrentFftWavesCascades}_{turbulence}.txt", finalText);



            //turbulence = 0.5f;
            //finalText  = "";
            //for (int idx = 0; idx <= KWS_Settings.FFT.MaxWindSpeed; idx++)
            //{
            //    waterInstance.Settings.GlobalWindSpeed        = idx;
            //    waterInstance.Settings.GlobalWindTurbulence = turbulence;
            //    waterInstance.ForceUpdateWaterSettings();

            //    float lastMaxHeight = 0;
            //    int   maxFrames     = (int)Mathf.Lerp(200, 2000, (1f * idx / KWS_Settings.FFT.MaxWindSpeed));
            //    for (int frame = 0; frame < maxFrames; frame++)
            //    {
            //        UpdateHeight(waterInstance, ref lastMaxHeight);
            //        Debug.Log($"idx {idx}  frame {frame}");
            //        yield return new WaitForSeconds(buoyancyFps);
            //    }

            //    if (idx == 0) lastMaxHeight = 0.001f;
            //    var maxHeightText           = lastMaxHeight.ToString().Replace(',', '.');

            //    finalText += $"{idx}, {maxHeightText}" + Environment.NewLine;
            //}

            Debug.Log("Finished " + turbulence);
            File.WriteAllText($@"D:\DebugWaterAmplitude_{waterInstance.Settings.CurrentFftWavesCascades}_{turbulence}.txt", finalText);

            turbulence = 1.0f;
            finalText  = "";
            for (int idx = 0; idx <= KWS_Settings.FFT.MaxWindSpeed; idx++)
            {
                waterInstance.Settings.GlobalWindSpeed        = idx;
                waterInstance.Settings.GlobalWindTurbulence = turbulence;
                waterInstance.ForceUpdateWaterSettings();

                float lastMaxHeight = 0;
                int   maxFrames     = (int)Mathf.Lerp(200, 2000, (1f * idx / KWS_Settings.FFT.MaxWindSpeed));
                for (int frame = 0; frame < maxFrames; frame++)
                {
                    UpdateHeight(waterInstance, ref lastMaxHeight);
                    Debug.Log($"idx {idx}  frame {frame}");
                    yield return new WaitForSeconds(buoyancyFps);
                }

                if (idx == 0) lastMaxHeight = 0.001f;
                var maxHeightText           = lastMaxHeight.ToString().Replace(',', '.');

                finalText += $"{idx}, {maxHeightText}" + Environment.NewLine;
            }

            Debug.Log("Finished " + turbulence);
            File.WriteAllText($@"D:\DebugWaterAmplitude_{waterInstance.Settings.CurrentFftWavesCascades}_{turbulence}.txt", finalText);
        }

        public static void UpdateHeight(WaterSystem waterInstance, ref float lastMaxHeight)
        {
            WaterSystem.GetWaterSurfaceData(Vector3.zero);
            if (WaterSharedResources.FftGpuHeightAsyncData == null) return;

            var asyncData = WaterSharedResources.FftGpuHeightAsyncData;
            var buffer    = asyncData.CurrentBuffer();
            var maxValue  = buffer.Max(n => n.height);

            lastMaxHeight = Mathf.Max(lastMaxHeight, maxValue);
        }


        public static void Release()
        {
            KW_Extensions.SafeDestroy(_debugTextureSliceMaterial);
        }

    }
}
